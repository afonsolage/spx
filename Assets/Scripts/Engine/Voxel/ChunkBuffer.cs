
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

    public void bind(ChunkBuffer buffer)
    {
        this.buffer = buffer;
    }

    public void target(int x, int y, int z)
    {
        if (pos == null)
            pos = new Vec3(x, y, z);
        else
            pos.set(x, y, z);

        updateOffset();
    }

    public void updateOffset()
    {
        this.offset = pos.ToVoxelOffset();
    }

    public Vec3 getPos()
    {
        return this.pos;
    }

    public void zeroFill()
    {
        for (int i = 0; i < Voxel.BYTE_NUM; i++)
            buffer.setByte(offset, i, 0);
    }

    public void setType(ushort type)
    {
        buffer.setShort(offset, Voxel.TYPE, type);
    }

    public ushort getType()
    {
        return buffer.getShort(offset, Voxel.TYPE);
    }
}

public class ChunkBuffer
{
    /** Total size of buffer that holds voxel data. */
    public static readonly int BUFFER_SIZE = (Chunk.SIZE ^ 3) * Voxel.BYTE_NUM;

    private byte[] voxels;

    public void allocate()
    {
        this.voxels = new byte[BUFFER_SIZE];
    }

    public void free()
    {
        this.voxels = null;
    }

    public void setByte(int voxelOffset, int data, byte value)
    {
        this.voxels[voxelOffset + data] = value;
    }

    public byte getByte(int voxelOffset, int data)
    {
        return this.voxels[voxelOffset + data];
    }

    public void setShort(int voxelOffset, int data, ushort value)
    {
        setByte(voxelOffset, data, (byte)((value >> 8) & 0xFF));
        setByte(voxelOffset, data + 1, (byte)(value & 0xFF));
    }

    public ushort getShort(int voxelOffset, int data)
    {
        return (ushort)((getByte(voxelOffset, data) << 8) | getByte(voxelOffset, data + 1));
    }
}
