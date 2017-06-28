using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    private static readonly int SIZE = 16;

    private byte[,,] voxels;
    private Vec3 pos;
	private Mesh mesh;

    public Chunk(Vec3 pos)
    {
        this.pos = pos;
        this.voxels = new byte[SIZE, SIZE, SIZE];
    }

    public void setup()
    {
        for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
                for (int z = 0; z < SIZE; z++)
                {
					voxels[x, y, z] = 1;
                }
    }

	public void build()
	{

	}
}