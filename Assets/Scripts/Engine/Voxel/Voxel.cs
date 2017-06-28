using System.Collections;
using System.Collections.Generic;
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
    public static readonly int RIGHT = 1;
    /**
	 * Back side byte position on Voxel data structure.
	 */
    public static readonly int BACK = 2;
    /**
	 * Left side byte position on Voxel data structure.
	 */
    public static readonly int LEFT = 3;
    /**
	 * Top side byte position on Voxel data structure.
	 */
    public static readonly int TOP = 4;
    /**
	 * Down side byte position on Voxel data structure.
	 */
    public static readonly int DOWN = 5;
    /**
	 * Type data byte position on Voxel data structure.
	 */
    public static readonly int TYPE = 6;
    /**
	 * Light data byte position on Voxel data structure.
	 */
    public static readonly int LIGHT = 8;
    /**
    * Indicates which one is the last byte in a voxel buffer data.
    */
    public static readonly int LAST_BYTE = LIGHT;
    /**
	 * An array containing all sides positions on a voxel data structure.
	 */
    public static readonly int[] ALL_SIDES = { FRONT, RIGHT, BACK, LEFT, TOP, DOWN };

    /**
	 * Size in bytes of Voxel data struct.
	 */
    public static readonly int BYTE_NUM = LAST_BYTE + 1;

    /* This is the a 3D cube ASCII representation to help to understand the bellow methods.																					
	 * 																							
	 *      v7 +-------+ v6																			
	 *      / |     /  |	 																	
	 *   v3 +-------+v2|																		
	 *      |v4+-------+ v5																		
	 *      | /     | /																			
	 *      +-------+ 																			
	 *     v0        v1																			
	 *     																						
	 *     Y																					
	 *     |  Z																					
	 *     | /																					
	 *     +----x																				
	 */

    /**
         * Compute the v0 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v0(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x;
        buffer[offset + 1] = y;
        buffer[offset + 2] = z + 1;
    }

    /**
         * Compute the v1 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v1(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x + 1;
        buffer[offset + 1] = y;
        buffer[offset + 2] = z + 1;
    }

    /**
         * Compute the v2 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v2(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x + 1;
        buffer[offset + 1] = y + 1;
        buffer[offset + 2] = z + 1;
    }

    /**
         * Compute the v3 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v3(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x;
        buffer[offset + 1] = y + 1;
        buffer[offset + 2] = z + 1;
    }

    /**
         * Compute the v4 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v4(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x;
        buffer[offset + 1] = y;
        buffer[offset + 2] = z;
    }

    /**
         * Compute the v5 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v5(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x + 1;
        buffer[offset + 1] = y;
        buffer[offset + 2] = z;
    }

    /**
         * Compute the v6 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v6(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x + 1;
        buffer[offset + 1] = y + 1;
        buffer[offset + 2] = z;
    }

    /**
         * Compute the v7 of a 3D cube using the given center of cube. This function doesn't check if the given buffer is valid or can hold the data.
         * 
         * @param x
         *            X position of the center of cube.
         * @param y
         *            Y position of the center of cube.
         * @param z
         *            Z position of the center of cube.
         * @param buffer
         *            A valid buffer to place the data into. The buffer lenght must be at least offset + 3.
         * @oaram offset
		 *			  Offset to place the data into the buffer.
         */
    public static void v7(int x, int y, int z, float[] buffer, int offset)
    {
        buffer[offset] = x;
        buffer[offset + 1] = y + 1;
        buffer[offset + 2] = z;
    }
}
