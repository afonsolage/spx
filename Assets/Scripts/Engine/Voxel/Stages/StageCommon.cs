using UnityEngine;

public class ChunkCluster
{
    private readonly ChunkStage[] neigbors;
    private readonly Vec3 pos;

    public ChunkCluster(Vec3 pos)
    {
        this.pos = pos;
        this.neigbors = new ChunkStage[Vec3.ALL_DIRS.Length];
    }

    private int Find(Vec3 neighbor)
    {
        var v = (neighbor - pos) / Chunk.SIZE;
        for (int i = 0; i < neigbors.Length; i++)
        {
            if (Vec3.ALL_DIRS[i] == v)
            {
                return i;
            }
        }

        Debug.LogError("Chunk neighbor not found on cluster: " + v);

        return -1;
    }

    public void UpdateStage(Vec3 neighbor, ChunkStage stage)
    {
        neigbors[Find(neighbor)] = stage;
    }

    private bool IsOnLowerStage(ChunkStage currentStage, ChunkStage neighborStage)
    {
        return neighborStage < currentStage;
    }

    public bool CanChangeStage(ChunkStage currentStage)
    {
        foreach (ChunkStage neighborStage in neigbors)
        {
            if (neighborStage != ChunkStage.NONE && IsOnLowerStage(currentStage, neighborStage))
                return false;
        }
        return true;
    }
}

public enum ChunkStage
{
    NONE,
    INITIALIZE,
    LOAD,
    VISIBILITY,
    SUNLIGHT,
    LIGHTING,
    MERGE_FACES,
    DONE,
}

public abstract class ChunkBaseStage
{
    public readonly ChunkStage stage;

    protected bool _finished;
    protected readonly SharedData _sharedData;
    protected object _output;

    public ChunkBaseStage(ChunkStage stage, SharedData sharedData)
    {
        this.stage = stage;
        _finished = false;
        _sharedData = sharedData;
    }

    public bool finished { get { return _finished; } }
    public bool done { get { return stage == ChunkStage.DONE; } }
    public object output { get { return _output; } }
    public void Start()
    {
        OnStart();
    }

    public virtual void Dispatch(ChunkMessage msg) { Debug.LogError("Unexpected message received: " + msg.action); }
    protected abstract void OnStart();

    protected void Finish()
    {
        _finished = true;
    }
}

public class ChunkNoneStage : ChunkBaseStage
{
    public ChunkNoneStage(SharedData sharedData) : base(ChunkStage.NONE, sharedData) { }
    protected override void OnStart() { Finish(); }
}

public class ChunkDoneStage : ChunkBaseStage
{
    public ChunkDoneStage(SharedData sharedData) : base(ChunkStage.DONE, sharedData) { }
    protected override void OnStart() { Finish(); }
}

public class SharedData
{
    public readonly ChunkBuffer buffer;
    public readonly Vec3 pos;
    public int voxelCount;
    public ChunkController controller;

    public SharedData(ChunkController controller, ChunkBuffer buffer, Vec3 pos)
    {
        this.buffer = buffer;
        this.pos = pos;
        this.controller = controller;
    }
}

public class ChunkStageSwitcher
{
    private ChunkBaseStage _current;
    private ChunkCluster _cluster;
    private SharedData _sharedData;

    public ChunkStageSwitcher(SharedData sharedData)
    {
        _current = new ChunkNoneStage(sharedData);
        _cluster = new ChunkCluster(sharedData.pos);
        _sharedData = sharedData;
    }

    public ChunkStage Next()
    {
        return _current.stage + 1;
    }

    public void Dispatch(ChunkMessage msg)
    {
        switch (msg.action)
        {
            case ChunkAction.CHANGE_STAGE:
                ChageStage(msg as ChunkChangeStageMessage);
                break;
            case ChunkAction.NFY_STAGE_CHANGED:
                NeighborStageChanged(msg as ChunkNotifyStageChanged);
                break;
            default:
                _current.Dispatch(msg);
                break;
        }

        TryGoToNextStage();
    }

    private void NeighborStageChanged(ChunkNotifyStageChanged msg)
    {
        _cluster.UpdateStage(msg.pos, msg.stage);
    }

    private void ChageStage(ChunkChangeStageMessage msg)
    {
        if (!_cluster.CanChangeStage(_current.stage))
        {
            return;
        }

        _current = GetNextStage(msg.stage, _sharedData);
        _current.Start();

        BroadacstChunkStageChanged();
    }

    private void TryGoToNextStage()
    {
        if (CanChangeStage())
        {
            _sharedData.controller.Post(new ChunkChangeStageMessage(_sharedData.pos, _current.stage + 1));
        }
    }

    private bool CanChangeStage()
    {
        return _current.finished && !_current.done && _cluster.CanChangeStage(_current.stage);
    }

    private ChunkBaseStage GetNextStage(ChunkStage newStage, SharedData sharedData)
    {
        switch (newStage)
        {
            case ChunkStage.INITIALIZE:
                return new ChunkInitializeStage(sharedData);
            case ChunkStage.LOAD:
                return new ChunkLoadStage(sharedData);
            case ChunkStage.VISIBILITY:
                return new ChunkVisibilityStage(sharedData);
            case ChunkStage.SUNLIGHT:
                return new ChunkSunlightStage(sharedData);
            case ChunkStage.MERGE_FACES:
                return new ChunkMergeFacesStage(sharedData, _current.output);
            case ChunkStage.DONE:
                return new ChunkDoneStage(sharedData);
            default:
                return new ChunkNoneStage(sharedData);
        }
    }

    private void BroadacstChunkStageChanged()
    {
        foreach (Vec3 dir in Vec3.ALL_DIRS)
        {
            _sharedData.controller.Post(new ChunkNotifyStageChanged(
                                            _sharedData.pos,
                                            _sharedData.pos + (dir * Chunk.SIZE),
                                            _current.stage
                                        ));
        }
    }
}