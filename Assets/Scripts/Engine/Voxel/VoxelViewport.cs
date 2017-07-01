﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

using GameObjectMap = System.Collections.Generic.Dictionary<Vec3, UnityEngine.GameObject>;

[ExecuteInEditMode]
public class VoxelViewport : MonoBehaviour, IChunkMeshConsumer
{
    private static readonly uint MAX_ATTACH_PER_TICK = 10;
    private static readonly uint MAX_DETACH_PER_TICK = 20;

    internal class ControllerMessage
    {
        public Vec3 pos;
        public PrebuiltMesh mesh;

        public ControllerMessage(Vec3 pos, PrebuiltMesh mesh) { this.pos = pos; this.mesh = mesh; }
        public bool IsAttach() { return mesh != null; }
    }
    public Material voxelDiffuse;
    public Vector3 voxelArea;

    private ChunkController _controller;
    private GameObjectMap _map;
    private ConcurrentQueue<ControllerMessage> _controllerQueue;

    public void Start()
    {
        name = "Voxel Viewport";

        _controller = new ChunkController(this);
        _controllerQueue = new ConcurrentQueue<ControllerMessage>();
        _map = new GameObjectMap();

        var area = new Vec3(voxelArea / 2);

        for (int x = -area.x; x <= area.x; x++)
        {
            for (int y = -area.y; y <= area.y; y++)
            {
                for (int z = -area.z; z <= area.z; z++)
                {
                    _controller.Post(new Vec3(x * Chunk.SIZE, y * Chunk.SIZE, z * Chunk.SIZE), ChunkMsg.Action.CREATE, null);
                }
            }
        }

        _controller.Start();
    }

    public void Update()
    {
        int attachCnt = 0;
        int detachCnt = 0;
        ControllerMessage msg;

        while (_controllerQueue.TryDequeue(out msg))
        {
            if (msg.IsAttach())
            {
                addChunk(msg.pos, msg.mesh);
                attachCnt++;
            }
            else
            {
                removeChunk(msg.pos);
                detachCnt++;
            }


            if (attachCnt >= MAX_ATTACH_PER_TICK)
                break;
            else if (detachCnt >= MAX_DETACH_PER_TICK)
                break;
        }
    }

    private void addChunk(Vec3 pos, PrebuiltMesh prebuiltMesh)
    {
        GameObject go = new GameObject("Chunk " + pos);
        go.transform.position = new Vector3(pos.x, pos.y, pos.z);
        go.transform.parent = transform;

        var filter = go.AddComponent<MeshFilter>();
        var mesh = prebuiltMesh.ToMesh();
        filter.mesh = mesh;

        var materials = new Material[mesh.subMeshCount];
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            materials[i] = voxelDiffuse;
        }

        var meshRenderer = go.AddComponent<MeshRenderer>();
        meshRenderer.materials = materials;

        _map[pos] = go;
    }

    private void removeChunk(Vec3 pos)
    {
        var obj = _map[pos];

        if (obj)
        {
            Destroy(obj);
            _map.Remove(pos);
        }
    }

    void IChunkMeshConsumer.PostAttach(Vec3 pos, PrebuiltMesh mesh)
    {
        _controllerQueue.Enqueue(new ControllerMessage(pos, mesh));
    }

    void IChunkMeshConsumer.PostDetach(Vec3 pos)
    {
        _controllerQueue.Enqueue(new ControllerMessage(pos, null));
    }
}