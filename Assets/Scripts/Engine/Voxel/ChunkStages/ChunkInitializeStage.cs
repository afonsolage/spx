using System;

public class ChunkInitializeStage : ChunkBaseStage
{
    public ChunkInitializeStage(SharedData sharedData) : base(ChunkStage.INITIALIZE, sharedData) {}

    protected override void OnStart()
    {
        _sharedData.buffer.Allocate();
        Finish();
    }
}