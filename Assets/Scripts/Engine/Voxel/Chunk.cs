using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    public static readonly int SIZE = 16;

    private Vec3 pos;
    private ChunkBuffer buffer;
    private Mesh mesh;

    void Start()
    {
        this.pos = new Vec3(transform.position);
        this.buffer = new ChunkBuffer();
    }

    public void setup()
    {
        VoxRef voxRef = new VoxRef(this.buffer);
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    voxRef.target(x, y, z);
                    voxRef.setType(1); // TODO: Add types
                }
            }
        }
    }

    public void build()
    {
        
    }
}