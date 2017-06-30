using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Vec3Test {
	[Test]
    public void SetTest()
    {
		var v = new Vec3(0, 0, 0);

        v.Set(10, 11, 12);
        Assert.AreEqual(10, v.x);
        Assert.AreEqual(11, v.y);
        Assert.AreEqual(12, v.z);

        v.Set(10, 11, 13);
        Assert.AreEqual(10, v.x);
        Assert.AreEqual(11, v.y);
        Assert.AreEqual(13, v.z);

        v.Set(10, 12, 13);
        Assert.AreEqual(10, v.x);
        Assert.AreEqual(12, v.y);
        Assert.AreEqual(13, v.z);

        v.Set(11, 12, 13);
        Assert.AreEqual(11, v.x);
        Assert.AreEqual(12, v.y);
        Assert.AreEqual(13, v.z);
    }

    [Test]
    public void SetVector3Test()
    {
        var v = new Vec3(new Vector3(10.3322131f, 11.98f, 12.000001f));
        Assert.AreEqual(10, v.x);
        Assert.AreEqual(11, v.y);
        Assert.AreEqual(12, v.z);
    }

    [Test]
    public void ToVoxelOffsetTest()
    {
		var v = new Vec3(0, 0, 0);

        v.Set(10, 11, 12);
        Assert.AreEqual(2748, v.ToVoxelOffset());

        v.Set(10, 11, 13);
        Assert.AreEqual(2749, v.ToVoxelOffset());

        v.Set(10, 12, 13);
        Assert.AreEqual(2765, v.ToVoxelOffset());

        v.Set(11, 12, 13);
        Assert.AreEqual(3021, v.ToVoxelOffset());
    }
}
