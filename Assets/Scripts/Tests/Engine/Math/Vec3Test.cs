using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class Vec3Test {
	[Test]
    public void ConstructorTest()
    {
		var v = new Vec3(10, 11, 12);

        Assert.AreEqual(10, v.x);
        Assert.AreEqual(11, v.y);
        Assert.AreEqual(12, v.z);
    }

    [Test]
    public void AnotherConstructorTest()
    {
        var v = new Vec3(new Vector3(10.3322131f, 11.98f, 12.000001f));
        Assert.AreEqual(10, v.x);
        Assert.AreEqual(11, v.y);
        Assert.AreEqual(12, v.z);
    }

    [Test]
    public void ToVoxelOffsetTest()
    {
		var v = new Vec3(10, 11, 12);

        Assert.AreEqual(2748, v.ToVoxelOffset());
    }
}
