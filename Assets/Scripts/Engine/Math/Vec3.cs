using UnityEngine;

public class Vec3
{
    public int x;
    public int y;
    public int z;

    public Vec3() { }

    public Vec3(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Vec3(Vector3 pos)
    {
        this.x = Mathf.FloorToInt(pos.x);
        this.y = Mathf.FloorToInt(pos.y);
        this.z = Mathf.FloorToInt(pos.z);
    }

    public int ToVoxelOffset()
    {
        return (x << 8) | (y << 4) | z;
    }

	public void set(int x, int y, int z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}
}
