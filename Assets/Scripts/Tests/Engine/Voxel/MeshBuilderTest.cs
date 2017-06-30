using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class MeshBuilderTest
{

    [Test]
    public void ConstructorTest()
    {
        var builder = new MeshBuilder();
        Assert.NotNull(builder);
    }

    [Test]
    public void AddTest()
    {
        var builder = new MeshBuilder();

        builder.Add(1, 1, new float[] { 1, 1, 1 });
        builder.Add(2, 1, new float[] { 2, 2, 2 });
        builder.Add(1, 1, new float[] { 3, 3, 3 });
        builder.Add(1, 2, new float[] { 4, 4, 4 });

        Assert.AreEqual(builder.GetPositions().Count, 4);
    }

    [Test]
    public void GetPositionsTest()
    {
        var builder = new MeshBuilder();

        builder.Add(1, 1, new float[] { 1, 1, 1 });
        builder.Add(2, 1, new float[] { 2, 2, 2 });
        builder.Add(1, 1, new float[] { 3, 3, 3 });
        builder.Add(1, 2, new float[] { 4, 4, 4 });

        var list = builder.GetPositions();

        Assert.AreEqual(new Vector3[] {
            new Vector3(1, 1, 1), new Vector3(3, 3, 3), new Vector3(2, 2, 2), new Vector3(4, 4, 4)
            }, list.ToArray());
    }

    [Test]
    public void GetNormalsTest()
    {
        var builder = new MeshBuilder();

        builder.Add(1, 1, new float[] { 1, 1, 1 });
        builder.Add(2, 1, new float[] { 2, 2, 2 });
        builder.Add(1, 1, new float[] { 3, 3, 3 });
        builder.Add(1, 2, new float[] { 4, 4, 4 });

        var list = builder.GetNormals();

        Assert.AreEqual(new Vector3[] {
            Vector3.right, Vector3.right, Vector3.right, Vector3.back
            }, list.ToArray());
    }

    [Test]
    public void GetIndiciesTest()
    {
        var builder = new MeshBuilder();

        builder.Add(1, 1, new float[] { 1, 1, 1 });
        builder.Add(2, 1, new float[] { 1, 1, 1 });
        builder.Add(2, 1, new float[] { 2, 2, 2 });
        builder.Add(1, 1, new float[] { 2, 2, 2 });
        builder.Add(2, 1, new float[] { 3, 3, 3 });
        builder.Add(1, 1, new float[] { 3, 3, 3 });
        builder.Add(1, 1, new float[] { 4, 4, 4 });
        builder.Add(2, 1, new float[] { 4, 4, 4 });

        var list = builder.GetIndices();

        Assert.AreEqual(2, list.Count);

        var indices1 = list[0];
        var indices2 = list[1];

        Assert.AreEqual(new int[] { 0, 1, 2, 2, 3, 0 }, indices1);
        Assert.AreEqual(new int[] { 4, 5, 6, 6, 7, 4 }, indices2);
    }

    [Test]
    public void GetUVsTest()
    {
        var builder = new MeshBuilder();

        builder.Add(1, 1, new float[] { 1, 1, 1 });
        builder.Add(1, 1, new float[] { 2, 2, 2 });
        builder.Add(1, 1, new float[] { 3, 3, 3 });
        builder.Add(1, 1, new float[] { 4, 4, 4 });

        var list = builder.GetUVs();

        Assert.AreEqual(new Vector2[] {
            new Vector2(3, 0), new Vector2(0, 0), new Vector2(0, 9), new Vector2(3, 9)
            }, list.ToArray());
    }
}
