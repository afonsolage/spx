public class ChunkBuffer
{
    /** Total size of buffer that holds voxel data. */
    public static readonly int BUFFER_SIZE = Chunk.SIZE * Chunk.SIZE * Chunk.SIZE * Voxel.BYTE_NUM;

    private byte[] voxels;

    public void Allocate()
    {
        this.voxels = new byte[BUFFER_SIZE];
    }

    public void Free()
    {
        this.voxels = null;
    }

    public void SetByte(int voxelOffset, int data, byte value)
    {
        this.voxels[voxelOffset + data] = value;
    }

    public bool IsFlagSet(int voxelOffset, int data, byte flag)
    {
        return (GetByte(voxelOffset, data) & flag) == flag;
    }

    public void SetFlag(int voxelOffset, int data, byte flag, bool val)
    {
        if (val)
        {
            this.voxels[voxelOffset + data] |= flag;
        }
        else
        {
            this.voxels[voxelOffset + data] &= (byte)~flag;
        }
    }

    public byte GetByte(int voxelOffset, int data)
    {
        return this.voxels[voxelOffset + data];
    }

    public void SetShort(int voxelOffset, int data, ushort value)
    {
        SetByte(voxelOffset, data, (byte)((value >> 8) & 0xFF));
        SetByte(voxelOffset, data + 1, (byte)(value & 0xFF));
    }

    public ushort GetShort(int voxelOffset, int data)
    {
        return (ushort)((GetByte(voxelOffset, data) << 8) | GetByte(voxelOffset, data + 1));
    }
}
