using UnityEngine;

public class Vec3
{
    /**
    * Represents the right direction (1, 0, 0)
    */
    public static readonly Vec3 RIGHT = new Vec3(1, 0, 0);
    /**
	 * Represents the left direction (-1, 0, 0)
	 */
    public static readonly Vec3 LEFT = new Vec3(-1, 0, 0);

    /**
	 * Represents the up direction (0, 1, 0)
	 */
    public static readonly Vec3 UP = new Vec3(0, 1, 0);

    /**
	 * Represents the down direction (0, 1, 0)
	 */
    public static readonly Vec3 BOTTOM = new Vec3(0, -1, 0);

    /**
	 * Represents the forwards direction (0, 0, 1)
	 */
    public static readonly Vec3 FORWARD = new Vec3(0, 0, 1);

    /**
	 * Represents the backwards direction (0, 0, -1)
	 */
    public static readonly Vec3 BACKWARD = new Vec3(0, 0, -1);

    /**
	 * Represents a zero position (0, 0, 0)
	 */
    public static readonly Vec3 ZERO = new Vec3(0, 0, 0);

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

    public void Set(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
