using NUnit.Framework;

public class VoxSnap
{
    private int offset;
    private Vec3 pos;
    private bool[] visible;

    public VoxSnap(VoxRef voxRef)
    {
        pos = voxRef.GetPos().Clone();
        type = voxRef.type;
        _light = voxRef.GetRawLight();
        visible = new bool[Voxel.ALL_SIDES.Length];

        foreach (byte side in Voxel.ALL_SIDES)
            visible[side] = voxRef.IsVisible(side);
    }

    public bool IsVisible(byte side)
    {
        return visible[side];
    }

    public bool IsEmpty()
    {
        return type == Voxel.VT_EMPTY;
    }

    public Vec3 GetPos()
    {
        return this.pos;
    }

    public ushort type;

    private byte _light;
    /**
        The sun light value of current referenced voxel. Light values range from 0 to 15 (0xF)
    */
    public byte sunLight
    {
        get
        {
            return (byte)((_light & Voxel.LIGHT_SUN) >> Voxel.LIGHT_SUN_SHIFT);
        }
    }

    /**
    The normal (artificial) light value of current referenced voxel. Light values range from 0 to 15 (0xF)
    */
    public byte normalLight
    {
        get
        {
            return (byte)(_light & Voxel.LIGHT_NORMAL);
        }
    }

    /**
    * The light value (final) of current referenced voxel. This return the greater value of sun and normal light
    */
    public byte light
    {
        get
        {
            var sLight = sunLight;
            if (sLight == Voxel.SUNLIGHT_MAX_VALUE)
                return sLight;
            else
                return Calc.Max(sLight, normalLight);
        }
    }
}

public class VoxRef
{
    private static readonly Vec3[] SIDES_NORMALS = {
        Vec3.FORWARD,
        Vec3.RIGHT,
        Vec3.BACKWARD,
        Vec3.LEFT,
        Vec3.UP,
        Vec3.BOTTOM
    };

    private ChunkBuffer buffer;
    private int offset;
    private Vec3 pos = new Vec3();

    public VoxRef(ChunkBuffer buffer, Vec3 pos)
    {
        Bind(buffer);
        Target(pos.x, pos.y, pos.z);
    }

    public VoxRef(ChunkBuffer buffer)
    {
        Bind(buffer);
    }

    public VoxRef(ChunkBuffer buffer, int x, int y, int z)
    {
        Bind(buffer);
        Target(x, y, z);
    }

    public void Bind(ChunkBuffer buffer)
    {
        this.buffer = buffer;
    }

    public void Target(int x, int y, int z)
    {
        Target(new Vec3(x, y, z));
    }

    public void Target(Vec3 pos)
    {
        this.pos = pos;
        UpdateOffset();
    }

    public bool TryTarget(int x, int y, int z)
    {
        return TryTarget(new Vec3(x, y, z));
    }

    public bool IsValid()
    {
        return pos.x >= 0 && pos.x < Chunk.SIZE && pos.y >= 0 && pos.y < Chunk.SIZE && pos.z >= 0 && pos.z < Chunk.SIZE;
    }

    public bool TryTarget(Vec3 pos)
    {
        Target(pos);
        return IsValid();
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
            buffer.SetByte(offset + i, 0);
    }

    public VoxRef Clone()
    {
        return new VoxRef(buffer, pos);
    }

    public ushort type
    {
        get
        {
            return buffer.GetUShort(offset + Voxel.TYPE);
        }
        set
        {
            buffer.SetUShort(offset + Voxel.TYPE, value);
        }
    }

    /**
        The sun light value of current referenced voxel. Light values range from 0 to 15 (0xF)
    */
    public byte sunLight
    {
        get
        {
            return (byte)((buffer.GetByte(offset + Voxel.LIGHT) & Voxel.LIGHT_SUN) >> Voxel.LIGHT_SUN_SHIFT);
        }
        set
        {
            byte currentVal = sunLight; //Get current value
            currentVal &= (byte)~Voxel.LIGHT_SUN; //Reset sunlight value
            currentVal |= (byte)((value << Voxel.LIGHT_SUN_SHIFT) & Voxel.LIGHT_SUN);
            buffer.SetByte(offset + Voxel.LIGHT, currentVal);
        }
    }

    /**
    The normal (artificial) light value of current referenced voxel. Light values range from 0 to 15 (0xF)
    */
    public byte normalLight
    {
        get
        {
            return (byte)(buffer.GetByte(offset + Voxel.LIGHT) & Voxel.LIGHT_NORMAL);
        }
        set
        {
            byte currentVal = normalLight; //Get current value
            currentVal &= (byte)~Voxel.LIGHT_NORMAL; //Reset sunlight value
            currentVal |= (byte)(value & Voxel.LIGHT_NORMAL);
            buffer.SetByte(offset + Voxel.LIGHT, currentVal);
        }
    }

    /**
    * The light value (final) of current referenced voxel. This return the greater value of sun and normal light
    */
    public byte light
    {
        get
        {
            var sLight = sunLight;
            if (sLight == Voxel.SUNLIGHT_MAX_VALUE)
                return sLight;
            else
                return Calc.Max(sLight, normalLight);
        }
    }

    public byte GetRawLight()
    {
        return buffer.GetByte(offset + Voxel.LIGHT);
    }

    public bool IsVisible(byte side)
    {
        return buffer.IsFlagSet(offset + side, Voxel.MASK_VISIBLE);
    }

    public void SetVisible(byte side, bool visible)
    {
        buffer.SetFlag(offset + side, Voxel.MASK_VISIBLE, visible);
    }

    public bool IsEmpty()
    {
        return type == Voxel.VT_EMPTY;
    }

    public Vec3 SideDir(byte side)
    {
        return SIDES_NORMALS[side];
    }

    public VoxSnap Snapshot()
    {
        return new VoxSnap(this);
    }

    /* This is the a 3D cube ASCII representation to help to understand the bellow methods.																					
	 * 																							
	 *      v7 +-------+ v6																			
	 *       / |     / |	 																	
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
    public static float[] V0(int x, int y, int z)
    {
        return new float[] { x, y, z + 1 };
    }

    public float[] V0()
    {
        return V0(pos.x, pos.y, pos.z);
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
    public static float[] V1(int x, int y, int z)
    {
        return new float[] { x + 1, y, z + 1 };
    }

    public float[] V1()
    {
        return V1(pos.x, pos.y, pos.z);
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
    public static float[] V2(int x, int y, int z)
    {
        return new float[] { x + 1, y + 1, z + 1 };
    }

    public float[] V2()
    {
        return V2(pos.x, pos.y, pos.z);
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
    public static float[] V3(int x, int y, int z)
    {
        return new float[] { x, y + 1, z + 1 };
    }

    public float[] V3()
    {
        return V3(pos.x, pos.y, pos.z);
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
    public static float[] V4(int x, int y, int z)
    {
        return new float[] { x, y, z };
    }

    public float[] V4()
    {
        return V4(pos.x, pos.y, pos.z);
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
    public static float[] V5(int x, int y, int z)
    {
        return new float[] { x + 1, y, z };
    }

    public float[] V5()
    {
        return V5(pos.x, pos.y, pos.z);
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
    public static float[] V6(int x, int y, int z)
    {
        return new float[] { x + 1, y + 1, z };
    }

    public float[] V6()
    {
        return V6(pos.x, pos.y, pos.z);
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
    public static float[] V7(int x, int y, int z)
    {
        return new float[] { x, y + 1, z };
    }

    public float[] V7()
    {
        return V7(pos.x, pos.y, pos.z);
    }
}