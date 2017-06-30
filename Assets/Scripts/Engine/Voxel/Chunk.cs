using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public static readonly int SIZE = 16;

    // private Vec3 pos;
    private ChunkBuffer buffer;
    private MeshFilter filter;
    private Renderer meshRenderer;

    private Material matDiff;

    void Start()
    {
        //this.pos = new Vec3(transform.position);
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
        var noise = new SimplexNoiseGenerator();
        VoxRef voxRef = new VoxRef(this.buffer, new Vec3());
        for (int x = 0; x < SIZE; x++)
        {
            for (int z = 0; z < SIZE; z++)
            {
                float height = (noise.coherentNoise(x, 0, z, SIZE) + 1);
                for (int y = 0; y < height; y++)
                {
                    voxRef.Target(x, y, z);
                    voxRef.type = 1; // TODO: Add types
                }
            }
        }

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