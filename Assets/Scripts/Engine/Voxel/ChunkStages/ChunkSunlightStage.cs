using System.Collections.Generic;
using UnityEngine;

public class ChunkSunlightStage : ChunkBaseStage
{
    private Queue<Vec3> _propQueue;

    public ChunkSunlightStage(SharedData sharedData) : base(ChunkStage.SUNLIGHT, sharedData)
    {
        _propQueue = new Queue<Vec3>();
    }

    protected override void OnStart()
    {
        if (_sharedData.pos.y == _sharedData.controller.bounds.up.y * Chunk.SIZE)
        {
            VoxRef vox = new VoxRef(_sharedData.buffer);

            var y = Chunk.SIZE - 1;
            for (int x = 0; x < Chunk.SIZE; x++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    vox.Target(x, y, z);

                    if (vox.type == Voxel.VT_EMPTY)
                    {
                        vox.sunLight = Voxel.SUNLIGHT_MAX_VALUE;
                        _propQueue.Enqueue(vox.GetPos() + Vec3.BOTTOM);
                    }
                }
            }

            PropagateSunlight();
        }
    }

    public override void Dispatch(ChunkMessage msg)
    {
        switch (msg.action)
        {
            case ChunkAction.SUNLIGHT_PROP:
                AddToPropagationQueue(msg as ChunkSunlightPropagationMessage);
                break;
            case ChunkAction.SUNLIGHT_PROP_END:
                Finish();
                break;
            default:
                base.Dispatch(msg);
                break;
        }
    }

    private void AddToPropagationQueue(ChunkSunlightPropagationMessage msg)
    {
        msg.voxels.ForEach((v) => _propQueue.Enqueue(v));

        PropagateSunlight();
    }

    private void PropagateSunlight()
    {
        List<Vec3> propToBottomChunk = new List<Vec3>();

        VoxRef vox = new VoxRef(_sharedData.buffer);
        while (_propQueue.Count > 0)
        {
            if (vox.TryTarget(_propQueue.Dequeue()))
            {
                if (vox.type == Voxel.VT_EMPTY)
                {
                    vox.sunLight = Voxel.SUNLIGHT_MAX_VALUE;
                    _propQueue.Enqueue(vox.GetPos() + Vec3.BOTTOM);
                }
            }
            else
            {
                var pos = vox.GetPos();
                propToBottomChunk.Add(new Vec3(pos.x, Chunk.SIZE - 1, pos.z));
            }
        }

        // If there aren't any more propagation to downwards, tell bellow chunks to go to next stage, 
        // because no direct sunlight will reach there.
        if (propToBottomChunk.Count == 0)
        {
            for (int y = _sharedData.pos.y - Chunk.SIZE, endY = _sharedData.controller.bounds.bottom.y; y >= endY; y -= Chunk.SIZE)
            {
                _sharedData.controller.Post(new ChunkSunlightPropagationEndMessage(_sharedData.pos, new Vec3(_sharedData.pos.x, y, _sharedData.pos.z)));
            }
        }
        else
        {
            _sharedData.controller.Post(new ChunkSunlightPropagationMessage(_sharedData.pos, _sharedData.pos + (Vec3.BOTTOM * Chunk.SIZE), propToBottomChunk));
        }

        Finish();
    }

}