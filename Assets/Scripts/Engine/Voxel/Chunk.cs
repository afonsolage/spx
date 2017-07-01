using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public static readonly int SIZE = 16;

    private Vec3 pos;
    private ChunkBuffer buffer;
    private MeshFilter filter;
    private Renderer meshRenderer;

    private Material matDiff;

    void Start()
    {
        this.pos = new Vec3(transform.position);
        this.name = "Chunk " + pos.x + " " + pos.y + " " + pos.z;
        this.buffer = new ChunkBuffer();
        this.filter = GetComponent<MeshFilter>();
        this.meshRenderer = GetComponent<MeshRenderer>();

        StartCoroutine(Setup());
    }

    public void SetDiffuseMaterial(Material mat)
    {
        this.matDiff = mat;
    }

    public IEnumerator Setup()
    {
        this.buffer.Allocate();
        VoxRef voxRef = new VoxRef(this.buffer, new Vec3());


        var min = 0.0f;
        var max = 0.0f;
        var wx = pos.x;
        for (int x = 0; x < SIZE; x++, wx++)
        {
            var wz = pos.z;
            for (int z = 0; z < SIZE; z++, wz++)
            {
                float height = (float)(MakeSomeNoise.Get(wx, 0, wz, 7 / 1000.0, 4, 0.4f) * SIZE);
                if (height < min)
                    min = height;
                if (height > max)
                    max = height;

                var wy = pos.y;
                for (int y = 0; wy < height; y++, wy++)
                {
                    voxRef.Target(x, y, z);
                    voxRef.type = 1; // TODO: Add types
                }
            }
        }

        Debug.Log(min + " - " + max);

        Build();

        yield return null;
    }

    private void CheckVisibleFaces()
    {
        VoxRef voxRef = new VoxRef(buffer);
        VoxRef neighborRef = new VoxRef(buffer);
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

        var builder = new FacesMerger(buffer).Merge();

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
            materials[i] = matDiff;
        }

        meshRenderer.materials = materials;
        filter.mesh = mesh;
    }
}