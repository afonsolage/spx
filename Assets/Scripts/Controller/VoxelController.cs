using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class VoxelController : MonoBehaviour
{
    public Material voxelDiffuse;
    public Vector3 voxelArea;

    public void Start()
    {
        name = "Voxel Controller";

        var area = new Vec3(voxelArea / 2);

        for (int x = -area.x; x <= area.x; x++)
        {
            for (int y = -area.y; y <= area.y; y++)
            {
                for (int z = -area.z; z <= area.z; z++)
                {
                    addChunk(x, y, z);
                }
            }
        }
    }

    private void addChunk(int x, int y, int z)
    {
        GameObject go = new GameObject();
        go.transform.position = new Vector3(x * Chunk.SIZE, y * Chunk.SIZE, z * Chunk.SIZE);
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.AddComponent<Chunk>().SetDiffuseMaterial(voxelDiffuse);

        go.transform.parent = transform;
    }
}
