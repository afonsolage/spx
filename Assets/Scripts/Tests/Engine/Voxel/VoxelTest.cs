using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class VoxelTest
{
    [Test]
    public static void ByteNumTest()
    {
        Assert.Greater(Voxel.BYTE_NUM, Voxel.LAST_BYTE, "BYTE_NUM should always be greater than LAST_BYTE, since BYTE_NUM is the byte count.");
    }

    [Test]
    public static void ByteSidesTest()
    {
        Assert.AreEqual(6, Voxel.ALL_SIDES.Length, "Should have only 6 sides on a cube.");

        foreach (byte b in Voxel.ALL_SIDES)
        {
            //There can be only one of each.
            Assert.AreEqual(1, System.Array.FindAll(Voxel.ALL_SIDES, (s) => s == b).Length, "The side values should be unique");
        }
    }
}
