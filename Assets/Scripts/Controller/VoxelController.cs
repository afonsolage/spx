using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChunkMap = System.Collections.Generic.Dictionary<Vec3, Chunk>;
using ChunkKeyValuePair = System.Collections.Generic.KeyValuePair<Vec3, Chunk>;

[ExecuteInEditMode]
public class VoxelController : MonoBehaviour
{
    public Material voxelDiffuse;
    public Vector3 voxelArea;

    private ChunkMap chunkMap;

    public void Start()
    {
        chunkMap = new ChunkMap((int)voxelArea.x * (int)voxelArea.y * (int)voxelArea.z);
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
        var chunk = go.AddComponent<Chunk>().Init(this, voxelDiffuse, new Vec3(go.transform.position));

        go.transform.parent = transform;

        chunkMap[chunk.position] = chunk;
    }

    public Chunk GetChunk(Vec3 pos)
    {
        Chunk res = null;
        chunkMap.TryGetValue(pos, out res);
        return res;
    }
}
