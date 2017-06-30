using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Voxel
{
    /**
    * Front side byte position on Voxel data structure.
    */
    public static readonly byte FRONT = 0;
    /**
	 * Right side byte position on Voxel data structure.
	 */
    public static readonly byte RIGHT = 1;
    /**
	 * Back side byte position on Voxel data structure.
	 */
    public static readonly byte BACK = 2;
    /**
	 * Left side byte position on Voxel data structure.
	 */
    public static readonly byte LEFT = 3;
    /**
	 * Top side byte position on Voxel data structure.
	 */
    public static readonly byte TOP = 4;
    /**
	 * Down side byte position on Voxel data structure.
	 */
    public static readonly byte DOWN = 5;
    /**
	 * Type data byte position on Voxel data structure.
	 */
    public static readonly byte TYPE = 6;
    /**
	 * Light data byte position on Voxel data structure.
	 */
    public static readonly byte LIGHT = 8;
    /**
    * Indicates which one is the last byte in a voxel buffer data.
    */
    public static readonly byte LAST_BYTE = LIGHT;
    /**
	 * An array containing all sides positions on a voxel data structure.
	 */
    public static readonly byte[] ALL_SIDES = { FRONT, RIGHT, BACK, LEFT, TOP, DOWN };

    /**
	 * Size in bytes of Voxel data struct.
	 */
    public static readonly int BYTE_NUM = LAST_BYTE + 1;

    /**
    * BitWise check to be applied on a voxel side to check if it is visible.
    */
    public static readonly byte MASK_VISIBLE = 0x01; // 0000 0001
    
    /**
    * BitWise check to be applied on a voxel side to check if it is already merged.
    */
    public static readonly byte MASK_MERGED = 0x02; // 0000 0010

    /**
    * BitWise get light value of a voxel side.
    */
    public static readonly byte MASK_LIGHT = 0x3C; // 0011 1100

    /**
    * Empty Voxel Type.
    */
    public static readonly ushort VT_EMPTY = 0;
}
