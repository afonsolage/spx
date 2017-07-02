using System.Threading;
using System.Collections.Concurrent;

using ChunkMap = System.Collections.Generic.Dictionary<Vec3, Chunk>;
using UnityEngine;

public interface IChunkMeshConsumer
{
    void PostAttach(Vec3 pos, PrebuiltMesh mesh);
    void PostDetach(Vec3 pos);
}

public class ChunkController
{
    private Thread _mainThread;
    private BlockingCollection<ChunkMessage> _queue;
    private IChunkMeshConsumer _consumer;
    private ChunkMap _map;

    private bool _running;

    public ChunkController(IChunkMeshConsumer consumer)
    {
        _consumer = consumer;
        _queue = new BlockingCollection<ChunkMessage>();
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

    public void Post(ChunkMessage msg)
    {
        _queue.Add(msg);
    }

    public void Post(Vec3 pos, ChunkAction action)
    {
        Post(new ChunkMessage(pos, action));
    }

    private void Run()
    {
        while (_running)
        {
            try
            {
                Dispatch(_queue.Take());
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void Dispatch(ChunkMessage msg)
    {
        switch (msg.action)
        {
            case ChunkAction.CREATE:
                CreateChunk(msg.pos);
                break;
            case ChunkAction.ATTACH:
                AttachChunk(msg.pos, (msg as ChunkAttachMessage)?.mesh);
                break;
            case ChunkAction.DETACH:
                DetachChunk(msg.pos);
                break;
            default:
                {
                    Chunk dst;
                    if (msg is ChunkToChunkMessage)
                    {
                        var ctcMsg = msg as ChunkToChunkMessage;

                        if (_map.TryGetValue(ctcMsg.target, out dst))
                            dst.Dispatch(msg);
                        else if (_map.TryGetValue(ctcMsg.pos, out dst))
                            dst.Dispatch(ctcMsg.ToChunkNotFoundMessage());
                    }
                    else if (_map.TryGetValue(msg.pos, out dst))
                    {
                        dst.Dispatch(msg);
                    }   
                }
                break;
        }
    }

    private void CreateChunk(Vec3 pos)
    {
        Chunk c = new Chunk(this, pos);
        _map[pos] = c;
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