using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class VoxRefTest
{
    ChunkBuffer buffer;
    Vec3 v;

    [SetUp]
    public void Setup()
    {
        v = new Vec3(1, 1, 1);

        buffer = new ChunkBuffer();
        buffer.Allocate();
    }

    [Test]
    public void ConstructorTest()
    {
        var voxRef = new VoxRef(buffer, v);
        Assert.AreEqual(0, voxRef.type);

        voxRef.type = 11;

        Assert.AreEqual(11, new VoxRef(buffer, v).type);
        Assert.AreEqual(11, new VoxRef(buffer, 1, 1, 1).type);
        Assert.AreNotEqual(11, new VoxRef(buffer).type);
    }

    [Test]
    public void ChangeTypeTest()
    {
        var voxRef = new VoxRef(buffer, v);

        voxRef.type = 15;
        Assert.AreEqual(15, voxRef.type);
    }

    [Test]
    public void BindTest()
    {
        // Since we are creating a non-allocated buffer, 
        // if bind fails it'll give a null pointer exception.
        ChunkBuffer other = new ChunkBuffer();
        var voxRef = new VoxRef(other, v);

        voxRef.Bind(buffer);

        Assert.AreEqual(0, voxRef.type);
    }

    [Test]
    public void TargetTest()
    {
        var voxRef = new VoxRef(buffer, new Vec3(2, 3, 4));
        voxRef.type = 15;
        voxRef = new VoxRef(buffer);

        voxRef.Target(2, 3, 4);

        Assert.AreEqual(15, voxRef.type);
    }

    [Test]
    public void ZeroFillTest()
    {
        var voxRef = new VoxRef(buffer, v);
        voxRef.type = 15;

        voxRef.ZeroFill();

        Assert.AreEqual(0, voxRef.type);
    }

    [Test]
    public void V0to7Test()
    {
        var voxRef = new VoxRef(buffer, v);

        Assert.AreEqual(new float[] { 1, 1, 2 }, voxRef.V0());
        Assert.AreEqual(new float[] { 2, 1, 2 }, voxRef.V1());
        Assert.AreEqual(new float[] { 2, 2, 2 }, voxRef.V2());
        Assert.AreEqual(new float[] { 1, 2, 2 }, voxRef.V3());
        Assert.AreEqual(new float[] { 1, 1, 1 }, voxRef.V4());
        Assert.AreEqual(new float[] { 2, 1, 1 }, voxRef.V5());
        Assert.AreEqual(new float[] { 2, 2, 1 }, voxRef.V6());
        Assert.AreEqual(new float[] { 1, 2, 1 }, voxRef.V7());
    }

    [Test]
    public void VisibleChangeTest()
    {
        var voxRef = new VoxRef(buffer, v);

        var b4 = voxRef.IsVisible(Voxel.FRONT);
        voxRef.SetVisible(Voxel.FRONT, true);

        Assert.AreEqual(false, b4);
        Assert.AreEqual(true, voxRef.IsVisible(Voxel.FRONT));
		Assert.AreEqual(false, voxRef.IsVisible(Voxel.RIGHT));
    }
}
