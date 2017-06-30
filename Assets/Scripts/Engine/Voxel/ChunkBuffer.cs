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

    public void SetByte(int offset, byte value)
    {
        this.voxels[offset] = value;
    }

    public bool IsFlagSet(int offset, byte flag)
    {
        return (GetByte(offset) & flag) == flag;
    }

    public void SetFlag(int offset, byte flag, bool val)
    {
        if (val)
        {
            this.voxels[offset] |= flag;
        }
        else
        {
            this.voxels[offset] &= (byte)~flag;
        }
    }

    public byte GetByte(int offset)
    {
        return this.voxels[offset];
    }

    public void SetUShort(int offset, ushort value)
    {
        SetByte(offset, (byte)((value >> 8) & 0xFF));
        SetByte(offset + 1, (byte)(value & 0xFF));
    }

    public ushort GetUShort(int offset)
    {
        return (ushort)((GetByte(offset) << 8) | GetByte(offset + 1));
    }
}
