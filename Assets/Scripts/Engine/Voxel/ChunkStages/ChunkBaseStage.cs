public class ChunkCluster
{
    private readonly ChunkStage[] neigbors;
    private readonly Vec3 pos;

    public ChunkCluster(Vec3 pos)
    {
        this.pos = pos;
        this.neigbors = new ChunkStage[Vec3.ALL_UNIT_DIRS.Length];
    }

    private int Find(Vec3 v)
    {
        for (int i = 0; i < neigbors.Length; i++)
            if (Vec3.ALL_DIRS[i] == v)
                return i;

        return -1;
    }

    public void UpdateStage(Vec3 neighbor, ChunkStage stage)
    {
        neigbors[Find(neighbor - pos)] = stage;
    }

    private bool IsOnLowerStage(ChunkStage currentStage, ChunkStage neighborStage)
    {
        return neighborStage < currentStage;
    }

    public bool CanChangeStage(ChunkStage currentStage)
    {
        foreach(ChunkStage neighborStage in neigbors)
        {
            if (IsOnLowerStage(currentStage, neighborStage))
                return false;
        }
        return true;
    }
}

public enum ChunkStage
{
    NONE,
    UNLOAD,
    INITIALIZE,
    LOAD,
    PRE_VISIBILITY,
    VISIBILITY,
    BUILD,
}

public abstract class ChunkBaseStage
{
    public readonly ChunkStage stage;

    public ChunkBaseStage(ChunkStage stage)
    {
        this.stage = stage;
    }

    public abstract void OnInit();

    public void Done()
    {

    }
}