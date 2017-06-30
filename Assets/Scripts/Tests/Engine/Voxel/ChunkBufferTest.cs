using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ChunkBufferTest
{

    [Test]
    public void ConstructorTest()
    {
        var buffer = new ChunkBuffer();

        Assert.NotNull(buffer);
    }

    [Test]
    public void NotAllocatedByDefaultTest()
    {
        var buffer = new ChunkBuffer();

        Assert.Catch(() => buffer.GetByte(0), "Chunk Buffer should not allocate by default");
    }

    [Test]
    public void AllocateTest()
    {
        var buffer = new ChunkBuffer();

        buffer.Allocate();

        Assert.AreEqual(0, buffer.GetByte(0));
        Assert.AreEqual(0, buffer.GetByte(ChunkBuffer.BUFFER_SIZE - 1));
    }

    [Test]
    public void FreeTest()
    {
        var buffer = new ChunkBuffer();

        buffer.Allocate();
        buffer.Free();

        Assert.Catch(() => buffer.GetByte(0));
    }

    [Test]
    public void ByteChangeTest()
    {
        var buffer = new ChunkBuffer();
        buffer.Allocate();

        buffer.SetByte(10, 14);

        Assert.AreEqual(14, buffer.GetByte(10));
        Assert.AreEqual(0, buffer.GetByte(11));
        Assert.AreEqual(0, buffer.GetByte(9));
    }

    [Test]
    public void UShortChangeTest()
    {
        var buffer = new ChunkBuffer();
        buffer.Allocate();

        buffer.SetUShort(10, 1658);

        Assert.AreEqual(1658, buffer.GetUShort(10));
        Assert.AreEqual(0, buffer.GetUShort(8));
        Assert.AreEqual(0, buffer.GetUShort(12));
        Assert.AreNotEqual(0, buffer.GetUShort(11));
    }

    [Test]
    public void FlagChangeTest()
    {
        var buffer = new ChunkBuffer();
        buffer.Allocate();

		buffer.SetByte(10, 23);
        buffer.SetFlag(10, 0x10, true);
        Assert.True(buffer.IsFlagSet(10, 0x10));
		buffer.SetFlag(10, 0x10, false);
		Assert.False(buffer.IsFlagSet(10, 0x10));
    }
}
