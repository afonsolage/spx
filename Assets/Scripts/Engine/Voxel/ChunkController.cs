using System.Threading;
using System.Collections.Concurrent;

using ChunkMap = System.Collections.Generic.Dictionary<Vec3, Chunk>;
using UnityEngine;

public class ChunkMsg
{
    public enum Action
    {
        CREATE,
        LOAD,
        SETUP,
        BUILD,
        ATTACH,
        DETACH,
    }

    public readonly Vec3 pos;
    public readonly Action action;
    public readonly object data;

    public ChunkMsg(Vec3 pos, Action action, object data) { this.pos = pos; this.action = action; this.data = data; }
}

public interface IChunkMeshConsumer
{
    void PostAttach(Vec3 pos, PrebuiltMesh mesh);
    void PostDetach(Vec3 pos);
}

public class ChunkController
{
    private Thread _mainThread;
    private BlockingCollection<ChunkMsg> _queue;
    private IChunkMeshConsumer _consumer;
    private ChunkMap _map;

    private bool _running;

    public ChunkController(IChunkMeshConsumer consumer)
    {
        _consumer = consumer;
        _queue = new BlockingCollection<ChunkMsg>();
        _mainThread = new Thread(Run);
    }

    public void Start()
    {
        _running = true;
        _map = new ChunkMap();
        _mainThread.Start();
    }

    public bool IsAlive()
    {
        return _mainThread.IsAlive;
    }

    public void Stop()
    {
        _running = false;
    }

    public void Post(ChunkMsg msg)
    {
        _queue.Add(msg);
    }

    public void Post(Vec3 pos, ChunkMsg.Action action, object data)
    {
        Post(new ChunkMsg(pos, action, data));
    }

    private void Run()
    {
        while (_running)
        {
            Dispatch(_queue.Take());
        }
    }

    private void Dispatch(ChunkMsg msg)
    {
        switch (msg.action)
        {
            case ChunkMsg.Action.CREATE:
                CreateChunk(msg.pos);
                break;
            case ChunkMsg.Action.LOAD:
                LoadChunk(msg.pos);
                break;
            case ChunkMsg.Action.SETUP:
                SetupChunk(msg.pos);
                break;
            case ChunkMsg.Action.BUILD:
                BuildChunk(msg.pos);
                break;
            case ChunkMsg.Action.ATTACH:
                AttachChunk(msg.pos, msg.data as PrebuiltMesh);
                break;
            case ChunkMsg.Action.DETACH:
                DetachChunk(msg.pos);
                break;
            default:
                Debug.LogWarning("Unimplemented action: " + msg.action);
                break;
        }
    }

    private void CreateChunk(Vec3 pos)
    {
        Chunk c = new Chunk(this, pos);
        _map[pos] = c;
    }

    private void LoadChunk(Vec3 pos)
    {
        _map[pos]?.Load();
    }

    private void SetupChunk(Vec3 pos)
    {
        _map[pos]?.Setup();
    }

    private void BuildChunk(Vec3 pos)
    {
        _map[pos]?.Build();
    }

    private void AttachChunk(Vec3 pos, PrebuiltMesh mesh)
    {
        _consumer.PostAttach(pos, mesh);
    }

    private void DetachChunk(Vec3 pos)
    {
        _consumer.PostDetach(pos);
    }
}