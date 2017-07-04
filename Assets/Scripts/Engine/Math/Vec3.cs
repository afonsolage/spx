using System;
using System.Collections.Generic;
using UnityEngine;

public class Vec3
{
    public static readonly Vec3 ZERO = new Vec3(0, 0, 0);

    public static readonly Vec3 FORWARD = new Vec3(0, 0, 1);
    public static readonly Vec3 BACKWARD = new Vec3(0, 0, -1);
    public static readonly Vec3 RIGHT = new Vec3(1, 0, 0);
    public static readonly Vec3 RIGHT_FORWARD = new Vec3(1, 0, 1);
    public static readonly Vec3 RIGHT_BACKWARD = new Vec3(1, 0, -1);
    public static readonly Vec3 RIGHT_UP = new Vec3(1, 1, 0);
    public static readonly Vec3 RIGHT_UP_FORWARD = new Vec3(1, 1, 1);
    public static readonly Vec3 RIGHT_UP_BACKWARD = new Vec3(1, 1, -1);
    public static readonly Vec3 RIGHT_BOTTOM_FORWARD = new Vec3(1, -1, 1);
    public static readonly Vec3 RIGHT_BOTTOM = new Vec3(1, -1, 0);
    public static readonly Vec3 RIGHT_BOTTOM_BACKWARD = new Vec3(1, -1, -1);
    public static readonly Vec3 LEFT = new Vec3(-1, 0, 0);
    public static readonly Vec3 LEFT_FORWARD = new Vec3(-1, 0, 1);
    public static readonly Vec3 LEFT_BACKWARD = new Vec3(-1, 0, -1);
    public static readonly Vec3 LEFT_UP_FORWARD = new Vec3(-1, 1, 1);
    public static readonly Vec3 LEFT_UP = new Vec3(-1, 1, 0);
    public static readonly Vec3 LEFT_UP_BACKWARD = new Vec3(-1, 1, -1);
    public static readonly Vec3 LEFT_BOTTOM_FORWARD = new Vec3(-1, -1, 1);
    public static readonly Vec3 LEFT_BOTTOM = new Vec3(-1, -1, 0);
    public static readonly Vec3 LEFT_BOTTOM_BACKWARD = new Vec3(-1, -1, -1);
    public static readonly Vec3 UP = new Vec3(0, 1, 0);
    public static readonly Vec3 UP_FORWARD = new Vec3(0, 1, 1);
    public static readonly Vec3 UP_BACKWARD = new Vec3(0, 1, -1);
    public static readonly Vec3 BOTTOM = new Vec3(0, -1, 0);
    public static readonly Vec3 BOTTOM_FORWARD = new Vec3(0, -1, 1);
    public static readonly Vec3 BOTTOM_BACKWARD = new Vec3(0, -1, -1);

    public static readonly Vec3[] ALL_UNIT_DIRS = new Vec3[] { FORWARD, RIGHT, BACKWARD, LEFT, UP, BOTTOM };
    public static readonly Vec3[] ALL_DIRS = new Vec3[] { FORWARD, BACKWARD, RIGHT, RIGHT_FORWARD, RIGHT_BACKWARD,
        RIGHT_UP, RIGHT_UP_FORWARD, RIGHT_UP_BACKWARD, RIGHT_BOTTOM_FORWARD, RIGHT_BOTTOM, RIGHT_BOTTOM_BACKWARD,
        LEFT, LEFT_FORWARD, LEFT_BACKWARD, LEFT_UP_FORWARD, LEFT_UP, LEFT_UP_BACKWARD, LEFT_BOTTOM_FORWARD, LEFT_BOTTOM,
        LEFT_BOTTOM_BACKWARD, UP, UP_FORWARD, UP_BACKWARD, BOTTOM, BOTTOM_FORWARD, BOTTOM_BACKWARD };
    
    public readonly int x;
    public readonly int y;
    public readonly int z;

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

    public Vec3 Clone()
    {
        return new Vec3(x, y, z);
    }

    public override int GetHashCode()
    {
        return unchecked((((((3 + x) << 7) + y) << 7) + z) << 7);
    }

    public override bool Equals(object obj)
    {
        var v = obj as Vec3;
        return obj != null && v.x == x && v.y == y && v.z == z;
    }

    public static Vec3 operator +(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
    }

    public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
    }

    public static Vec3 operator *(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(lhs.x * rhs.x, lhs.y * rhs.y, lhs.z * rhs.z);
    }

    public static Vec3 operator /(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(lhs.x / rhs.x, lhs.y / rhs.y, lhs.z / rhs.z);
    }

    public static Vec3 operator %(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(Calc.Mod(lhs.x, rhs.x), Calc.Mod(lhs.y, rhs.y), Calc.Mod(lhs.z, rhs.z));
    }

    public static bool operator ==(Vec3 lhs, Vec3 rhs)
    {
        if (object.ReferenceEquals(lhs, null))
        {
            return object.ReferenceEquals(rhs, null);
        }
        return lhs.Equals(rhs);
    }

    public static bool operator !=(Vec3 lhs, Vec3 rhs)
    {
        return !(lhs == rhs);
    }

    public static Vec3 operator *(Vec3 lhs, int val)
    {
        return new Vec3(lhs.x * val, lhs.y * val, lhs.z * val);
    }

    public static Vec3 operator %(Vec3 lhs, int val)
    {
        return new Vec3(Calc.Mod(lhs.x, val), Calc.Mod(lhs.y, val), Calc.Mod(lhs.z, val));
    }

    public static Vec3 operator /(Vec3 lhs, int val)
    {
        return new Vec3(lhs.x / val, lhs.y / val, lhs.z / val);
    }

    public override string ToString()
    {
        return x + ", " + y + ", " + z;
    }

}
