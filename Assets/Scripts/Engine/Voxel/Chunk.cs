using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Chunk : MonoBehaviour
{
    public static readonly int SIZE = 16;

    private VoxelController _parent;
    private Vec3 _pos;
    private ChunkBuffer _buffer;

    public Chunk[] _neighbors;

    private Material _matDiff;

    private ushort _voxelCount;

    public bool IsEmpty()
    {
        return _voxelCount == 0;
    }

    public Chunk Init(VoxelController parent, Material matDiff, Vec3 pos)
    {
        _parent = parent;
        _pos = pos;
        _matDiff = matDiff;

        _buffer = new ChunkBuffer();
        _neighbors = new Chunk[Vec3.ALL_DIRECTIONS.Length];

        this.name = "Chunk " + _pos;

        return this;
    }

    void Start()
    {
        LoadNeighbors();
        Setup();
    }

    public void LoadNeighbors()
    {
        int i = 0;
        foreach (Vec3 dir in Vec3.ALL_DIRECTIONS)
        {
            _neighbors[i++] = _parent.GetChunk(_pos + dir * Chunk.SIZE);
        }
    }

    public Vec3 position
    {
        get
        {
            return _pos;
        }
    }

    public void Setup()
    {
        _buffer.Allocate();
        VoxRef voxRef = new VoxRef(_buffer, new Vec3());
        var wx = _pos.x;

        for (int x = 0; x < SIZE; x++, wx++)
        {
            var wz = _pos.z;
            for (int z = 0; z < SIZE; z++, wz++)
            {
                float height = (float)(MakeSomeNoise.Get(wx, 0, wz, 7 / 1000.0, 4, 0.4f) * SIZE);
                var wy = _pos.y;
                for (int y = 0; wy < height; y++, wy++)
                {
                    voxRef.Target(x, y, z);
                    voxRef.type = 1; // TODO: Add types

                    //If there is at least on block on this chunk, than itsn't empty.
                    _voxelCount++;
                }
            }
        }

        if (IsEmpty())
        {
            _buffer.Free();
        }
        else
        {
            Build();
        }
    }

    private void CheckVisibleFaces()
    {
        VoxRef voxRef = new VoxRef(_buffer);
        VoxRef neighborRef = new VoxRef(_buffer);
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    voxRef.Target(x, y, z);

                    if (voxRef.IsEmpty())
                        continue;

                    foreach (byte side in Voxel.ALL_SIDES)
                    {
                        var dir = voxRef.SideDir(side);
                        var pos = voxRef.GetPos();

                        if (!neighborRef.TryTarget(pos.x + dir.x, pos.y + dir.y, pos.z + dir.z))
                            voxRef.SetVisible(side, true);
                        else
                            voxRef.SetVisible(side, neighborRef.IsEmpty());
                    }
                }
            }
        }
    }

    public void Build()
    {
        CheckVisibleFaces();

        var builder = new FacesMerger(_buffer).Merge();

        Mesh mesh = new Mesh();
        mesh.name = "Chunk Mesh";
        mesh.SetVertices(builder.GetPositions());
        mesh.SetNormals(builder.GetNormals());
        mesh.SetUVs(0, builder.GetUVs());
        mesh.SetUVs(1, builder.getTileUVs());
        mesh.SetColors(builder.getColors());

        var subMeshIndices = builder.GetIndices();
        mesh.subMeshCount = subMeshIndices.Count;
        var materials = new Material[mesh.subMeshCount];

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            mesh.SetIndices(subMeshIndices[i], MeshTopology.Triangles, i);
            materials[i] = _matDiff;
        }

        var filter = gameObject.AddComponent<MeshFilter>();
        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.materials = materials;
        filter.mesh = mesh;
    }
}