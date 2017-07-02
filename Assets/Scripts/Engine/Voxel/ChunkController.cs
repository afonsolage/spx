using System.Threading;
using System.Collections.Concurrent;

using ChunkMap = System.Collections.Generic.Dictionary<Vec3, Chunk>;
using UnityEngine;
using System;

public interface IChunkMeshConsumer
{
    void PostAttach(Vec3 pos, PrebuiltMesh mesh);
    void PostDetach(Vec3 pos);
}

public class ChunkController
{
    private Thread _mainThread;
    private BlockingCollection<ChunkMessage> _queue;
    private WeakReference _consumer;
    private ChunkMap _map;

    private bool _running;

    public ChunkController(IChunkMeshConsumer consumer)
    {
        _consumer = new WeakReference(consumer);
        _queue = new BlockingCollection<ChunkMessage>();
        _mainThread = new Thread(Run);
        _mainThread.Name = "ChunkController";
    }

    public void Start()
    {
        Debug.Log("Starting ChunkController thread.");
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
        if (_running)
        {
            Debug.Log("Stopping ChunkController thread.");
            _running = false;
            _mainThread.Interrupt();

            foreach (Chunk c in _map.Values)
                c.Unload();

            _map = null;
        }

        GC.Collect();
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
        while (_running && _consumer.IsAlive)
        {
            try
            {
                Dispatch(_queue.Take());
            }
            catch (ThreadAbortException)
            {
                Stop();
                return;
            }
            catch (ThreadInterruptedException)
            {
                Stop();
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
            }
        }
        Stop();
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
        (_consumer.Target as IChunkMeshConsumer)?.PostAttach(pos, mesh);
    }

    private void DetachChunk(Vec3 pos)
    {
        (_consumer.Target as IChunkMeshConsumer)?.PostDetach(pos);
    }
}