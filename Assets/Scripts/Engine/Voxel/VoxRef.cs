public class VoxRef
{
    private ChunkBuffer buffer;
    private int offset;
    private Vec3 pos;

    public VoxRef(ChunkBuffer buffer, Vec3 pos)
    {
        this.buffer = buffer;
        this.pos = pos;
    }

    public VoxRef(ChunkBuffer buffer)
    {
        this.buffer = buffer;
    }

    public void Bind(ChunkBuffer buffer)
    {
        this.buffer = buffer;
    }

    public void Target(int x, int y, int z)
    {
        if (pos == null)
            pos = new Vec3(x, y, z);
        else
            pos.set(x, y, z);

        UpdateOffset();
    }

    public void UpdateOffset()
    {
        this.offset = pos.ToVoxelOffset() * Voxel.BYTE_NUM;
    }

    public Vec3 GetPos()
    {
        return this.pos;
    }

    public void ZeroFill()
    {
        for (int i = 0; i < Voxel.BYTE_NUM; i++)
            buffer.SetByte(offset, i, 0);
    }


	public ushort type
	{
		get
		{
			return buffer.GetShort(offset, Voxel.TYPE);	
		}
		set
		{
			buffer.SetShort(offset, Voxel.TYPE, value);
		}
	}

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
    public float[] V0()
    {
        return new float[] { pos.x, pos.y, pos.z + 1 };
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
    public float[] V1()
    {
        return new float[] { pos.x + 1, pos.y, pos.z + 1 };
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
    public float[] V2()
    {
        return new float[] { pos.x + 1, pos.y + 1, pos.z + 1 };
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
    public float[] V3()
    {
        return new float[] { pos.x, pos.y + 1, pos.z + 1 };
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
    public float[] V4()
    {
        return new float[] { pos.x, pos.y, pos.z };
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
    public float[] V5()
    {
        return new float[] { pos.x + 1, pos.y, pos.z };
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
    public float[] V6()
    {
        return new float[] { pos.x + 1, pos.y + 1, pos.z };
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
    public float[] V7()
    {
        return new float[] { pos.x, pos.y + 1, pos.z };
    }

}