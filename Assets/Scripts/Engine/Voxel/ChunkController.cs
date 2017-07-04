using System.Threading;
using System.Collections.Concurrent;

using ChunkMap = System.Collections.Generic.Dictionary<Vec3, Chunk>;
using UnityEngine;
using System;
using System.Collections.Generic;

public interface IChunkMeshConsumer
{
    void PostAttach(Vec3 pos, PrebuiltMesh mesh);
    void PostDetach(Vec3 pos);
}

public class ChunkRunner
{
    class RunnerTask
    {
        public Chunk chunk;
        public ChunkMessage message;

        public RunnerTask(Chunk c, ChunkMessage msg) { this.chunk = c; this.message = msg; }
    }

    private Thread _thread;
    private bool _running;
    private BlockingCollection<RunnerTask> _tasks;
    private Vec3 _currentChunk;
    private ManualResetEventSlim _event;

    public ChunkRunner(string name, ManualResetEventSlim evt)
    {
        _tasks = new BlockingCollection<RunnerTask>(1);
        _thread = new Thread(Run);
        _thread.Name = name;
        _running = false;
        _currentChunk = Vec3.ZERO;
        _event = evt;
    }

    private void Run()
    {
        while (_running)
        {
            try
            {
                var task = _tasks.Take();
                _currentChunk = task.chunk.pos;
                task.chunk.Dispatch(task.message);
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
                if (_running)
                    Debug.LogException(e);
            }

            _event.Set();
        }
    }

    public void Start()
    {
        _running = true;
        _thread.Start();
    }

    public void Stop()
    {
        if (_running)
        {
            _running = false;
            _thread.Interrupt();
            _tasks.Dispose();
        }
    }

    public bool IsAlreadyRunning(Vec3 chunk)
    {
        return _currentChunk == chunk;
    }

    public bool IsFree()
    {
        return _tasks.Count == 0;
    }

    public void Add(Chunk chunk, ChunkMessage msg)
    {
        _tasks.Add(new RunnerTask(chunk, msg));
    }
}

public class ChunkController
{
    private static readonly int PARALLEL_RUNNERS = 8;

    private Thread _mainThread;
    private BlockingCollection<ChunkMessage> _queue;
    private ChunkRunner[] _runners;
    private ManualResetEventSlim _runnerEvent;

    private WeakReference _consumer;
    private ChunkMap _map;

    private bool _running;

    public ChunkController(IChunkMeshConsumer consumer)
    {
        _consumer = new WeakReference(consumer);
        _queue = new BlockingCollection<ChunkMessage>();
        _mainThread = new Thread(Run);
        _mainThread.Name = "ChunkController";
        _runners = new ChunkRunner[PARALLEL_RUNNERS];
        _runnerEvent = new ManualResetEventSlim(false);
    }

    public void Start()
    {
        Debug.Log("Starting ChunkController thread.");
        _running = true;
        _map = new ChunkMap();

        for (int i = 0; i < PARALLEL_RUNNERS; i++)
        {
            _runners[i] = new ChunkRunner("Chunk Runner " + i, _runnerEvent);
            _runners[i].Start();
        }

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
            _map.Clear();

            for (int i = 0; i < PARALLEL_RUNNERS; i++)
            {
                _runners[i].Stop();
            }

            _runners = null;
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
                SendToChunk(msg);
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

    private ChunkRunner GetRunner(Vec3 chunk)
    {
        foreach (ChunkRunner runner in _runners)
        {
            if (runner.IsAlreadyRunning(chunk))
                return runner;
        }

        do
        {
            foreach (ChunkRunner runner in _runners)
            {
                if (runner.IsFree())
                    return runner;
            }
            _runnerEvent.Wait(100);
        }
        while (true);
    }

    private void SendToChunk(ChunkMessage msg)
    {
        ChunkRunner runner = GetRunner(msg.pos);

        if (runner == null)
            return;

        Chunk dst;
        if (msg is ChunkToChunkMessage)
        {
            var ctcMsg = msg as ChunkToChunkMessage;

            if (_map.TryGetValue(ctcMsg.target, out dst))
                runner.Add(dst, msg);
            else if (_map.TryGetValue(ctcMsg.pos, out dst))
                runner.Add(dst, ctcMsg.ToChunkNotFoundMessage());
        }
        else if (_map.TryGetValue(msg.pos, out dst))
        {
            runner.Add(dst, msg);
        }
    }
}