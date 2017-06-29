﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public static readonly int SIZE = 16;

    // private Vec3 pos;
    private ChunkBuffer buffer;
    private MeshFilter filter;

    void Start()
    {
        //this.pos = new Vec3(transform.position);
        this.buffer = new ChunkBuffer();
        this.filter = GetComponent<MeshFilter>();
        
        StartCoroutine(Setup());
    }

    public IEnumerator Setup()
    {
        this.buffer.Allocate();
        VoxRef voxRef = new VoxRef(this.buffer);
        for (int x = 5; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE - 5; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    voxRef.Target(x, y, z);
                    voxRef.type = 1; // TODO: Add types
                }
            }
        }

        Build();

        yield return null;
    }

    public void Build()
    {
        var builder = new MeshBuilder();

        VoxRef voxRef = new VoxRef(this.buffer);
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    voxRef.Target(x, y, z);

                    if (voxRef.type == 0)
                        continue;

                    builder.Add(voxRef.type, Voxel.FRONT, voxRef.V0());
                    builder.Add(voxRef.type, Voxel.FRONT, voxRef.V1());
                    builder.Add(voxRef.type, Voxel.FRONT, voxRef.V2());
                    builder.Add(voxRef.type, Voxel.FRONT, voxRef.V3());

                    builder.Add(voxRef.type, Voxel.RIGHT, voxRef.V1());
                    builder.Add(voxRef.type, Voxel.RIGHT, voxRef.V5());
                    builder.Add(voxRef.type, Voxel.RIGHT, voxRef.V6());
                    builder.Add(voxRef.type, Voxel.RIGHT, voxRef.V2());

                    builder.Add(voxRef.type, Voxel.BACK, voxRef.V5());
                    builder.Add(voxRef.type, Voxel.BACK, voxRef.V4());
                    builder.Add(voxRef.type, Voxel.BACK, voxRef.V7());
                    builder.Add(voxRef.type, Voxel.BACK, voxRef.V6());

                    builder.Add(voxRef.type, Voxel.LEFT, voxRef.V4());
                    builder.Add(voxRef.type, Voxel.LEFT, voxRef.V0());
                    builder.Add(voxRef.type, Voxel.LEFT, voxRef.V3());
                    builder.Add(voxRef.type, Voxel.LEFT, voxRef.V7());

                    builder.Add(voxRef.type, Voxel.TOP, voxRef.V3());
                    builder.Add(voxRef.type, Voxel.TOP, voxRef.V2());
                    builder.Add(voxRef.type, Voxel.TOP, voxRef.V6());
                    builder.Add(voxRef.type, Voxel.TOP, voxRef.V7());

                    builder.Add(voxRef.type, Voxel.DOWN, voxRef.V4());
                    builder.Add(voxRef.type, Voxel.DOWN, voxRef.V5());
                    builder.Add(voxRef.type, Voxel.DOWN, voxRef.V1());
                    builder.Add(voxRef.type, Voxel.DOWN, voxRef.V0());
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "Chunk Mesh";
        mesh.subMeshCount = 1;
        mesh.SetVertices(builder.GetPositions());
        mesh.SetIndices(builder.GetIndices(), MeshTopology.Triangles, 0);
        mesh.SetNormals(builder.GetNormals());
        mesh.SetUVs(0, builder.GetUVs());
        mesh.SetUVs(1, builder.getTileUVs());
        mesh.SetColors(builder.getColors());

        filter.mesh = mesh;
    }
}