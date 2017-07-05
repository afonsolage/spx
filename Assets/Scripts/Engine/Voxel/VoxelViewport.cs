using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    public void OnDestroy()
    {
        Debug.Log("Destroying VoxelViewport");

        foreach (GameObject go in _map.Values)
        {
            Destroy(go);
        }

        _controller.Stop();
    }

    public void Start()
    {
        name = "Voxel Viewport";

        _controller = new ChunkController(this);
        _controllerQueue = new ConcurrentQueue<ControllerMessage>();
        _map = new GameObjectMap();

        _controller.Start(new Vec3(voxelArea / 2));
    }

    public void Update()
    {
        if (_controllerQueue == null)
            return;

        int attachCnt = 0;
        int detachCnt = 0;
        ControllerMessage msg;

        while (_controllerQueue.TryDequeue(out msg))
        {
            if (msg.IsAttach())
            {
                AddChunk(msg.pos, msg.mesh);
                attachCnt++;
            }
            else
            {
                RemoveChunk(msg.pos);
                detachCnt++;
            }


            if (attachCnt >= MAX_ATTACH_PER_TICK)
                break;
            else if (detachCnt >= MAX_DETACH_PER_TICK)
                break;
        }
    }

    private void AddChunk(Vec3 pos, PrebuiltMesh prebuiltMesh)
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

    private void RemoveChunk(Vec3 pos)
    {
        GameObject obj;
        if (_map.TryGetValue(pos, out obj))
        {
            Destroy(obj);
            _map.Remove(pos);
        }
    }

    void IChunkMeshConsumer.PostAttach(Vec3 pos, PrebuiltMesh mesh)
    {
        Debug.Assert(mesh != null);
        _controllerQueue.Enqueue(new ControllerMessage(pos, mesh));
    }

    void IChunkMeshConsumer.PostDetach(Vec3 pos)
    {
        _controllerQueue.Enqueue(new ControllerMessage(pos, null));
    }
}
