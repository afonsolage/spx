
using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static readonly int SIZE = 16;

    private readonly SharedData _data;
    private readonly ChunkStageSwitcher _switcher;

    private bool _empty;

    public Chunk(ChunkController controller, Vec3 pos)
    {
        _data = new SharedData(controller, new ChunkBuffer(), pos);
        _switcher = new ChunkStageSwitcher(_data);

        _data.controller.Post(new ChunkChangeStageMessage(_data.pos, ChunkStage.INITIALIZE));
    }

    public Vec3 pos { get { return _data.pos; } }

    public bool IsEmpty()
    {
        return _data.voxelCount == 0;
    }

    public void Dispatch(ChunkMessage msg)
    {
        switch (msg.action)
        {
            case ChunkAction.NONE:
                //do nothing.
                break;
            case ChunkAction.REQ_VOX:
                VoxRequest(msg as ChunkReqVoxMessage);
                break;
            default:
                _switcher.Dispatch(msg);
                break;
        }
    }

    private void VoxRequest(ChunkReqVoxMessage msg)
    {
        List<KeyValuePair<Vec3, VoxSnap>> result = null;

        if (!IsEmpty())
        {
            result = new List<KeyValuePair<Vec3, VoxSnap>>();

            var voxRef = new VoxRef(_data.buffer);
            foreach (Vec3 v in msg.voxels)
            {
                if (voxRef.TryTarget(v))
                    result.Add(new KeyValuePair<Vec3, VoxSnap>(v, voxRef.Snapshot()));
                else
                    result.Add(new KeyValuePair<Vec3, VoxSnap>(v, null));
            }
        }

        _data.controller.Post(new ChunkResVoxMessage(msg, result));
    }
}