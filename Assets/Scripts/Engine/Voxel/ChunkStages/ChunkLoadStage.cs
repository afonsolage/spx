using System;

public class ChunkLoadStage : ChunkBaseStage
{
    public ChunkLoadStage(SharedData sharedData) : base(ChunkStage.LOAD, sharedData) { }

    protected override void OnStart()
    {
        VoxRef voxRef = new VoxRef(_sharedData.buffer, new Vec3());
        var wx = _sharedData.pos.x;

        for (int x = 0; x < Chunk.SIZE; x++, wx++)
        {
            var wz = _sharedData.pos.z;
            for (int z = 0; z < Chunk.SIZE; z++, wz++)
            {
                float height = (float)(MakeSomeNoise.Get(wx, 0, wz, 7 / 1000.0, 2, 0.9f) * Chunk.SIZE);
                var wy = _sharedData.pos.y;
                for (int y = 0; wy < height; y++, wy++)
                {
                    voxRef.Target(x, y, z);
                    voxRef.type = 1; // TODO: Add types

                    //If there is at least on block on this chunk, then itsn't empty.
                    _sharedData.voxelCount++;
                }
            }
        }

        Finish();
    }
}