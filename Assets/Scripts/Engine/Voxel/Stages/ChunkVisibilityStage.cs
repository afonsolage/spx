using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkVisibilityStage : ChunkBaseStage
{
    class NeighborReq
    {
        public readonly Vec3 vox;
        public readonly Vec3 neighbor;
        public readonly byte side;

        public NeighborReq(Vec3 vox, Vec3 neighbor, byte side)
        {
            this.vox = vox;
            this.neighbor = neighbor;
            this.side = side;
        }
    }

    Dictionary<Vec3, List<NeighborReq>> chunkRequests = new Dictionary<Vec3, List<NeighborReq>>();

    public ChunkVisibilityStage(SharedData sharedData) : base(ChunkStage.VISIBILITY, sharedData)
    {
    }

    protected override void OnStart()
    {
        if (_sharedData.voxelCount == 0)
        {
            Finish();
            return;
        }

        VoxRef voxRef = new VoxRef(_sharedData.buffer);
        VoxRef ngborVoxRef = new VoxRef(_sharedData.buffer);

        //We won't check borders now, since we'll need the neightboor info
        for (int x = 0; x < Chunk.SIZE; x++)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    voxRef.Target(x, y, z);

                    if (voxRef.IsEmpty())
                        continue;

                    foreach (byte side in Voxel.ALL_SIDES)
                    {
                        var neighborPos = voxRef.SideNeighbor(side);

                        if (ngborVoxRef.TryTarget(neighborPos))
                        {
                            var visible = ngborVoxRef.IsEmpty();
                            voxRef.SetVisible(side, visible);
                        }
                        else
                        {
                            var neighborChunk = _sharedData.pos + Vec3.ALL_UNIT_DIRS[side] * Chunk.SIZE;
                            neighborPos %= Chunk.SIZE;

                            AddRequestChunkVoxel(voxRef.GetPos(), neighborChunk, neighborPos, side);
                        }
                    }
                }
            }
        }

        SendRequest();
    }

    public override void Dispatch(ChunkMessage msg)
    {
        switch (msg.action)
        {
            case ChunkAction.RES_VOX:
                ChunkVoxelRes(msg as ChunkResVoxMessage);
                break;
            default:
                base.Dispatch(msg);
                break;
        }
    }

    private void AddRequestChunkVoxel(Vec3 reqVox, Vec3 targetChunk, Vec3 targetVox, byte side)
    {
        List<NeighborReq> list;
        if (!chunkRequests.TryGetValue(targetChunk, out list))
        {
            list = new List<NeighborReq>();
            chunkRequests[targetChunk] = list;
        }
        list.Add(new NeighborReq(reqVox, targetVox, side));
    }

    private void SendRequest()
    {
        foreach (KeyValuePair<Vec3, List<NeighborReq>> pair in chunkRequests)
        {
            List<Vec3> list = new List<Vec3>();
            pair.Value.ForEach((r) => list.Add(r.neighbor));
            _sharedData.controller.Post(new ChunkReqVoxMessage(_sharedData.pos, pair.Key, list));
        }
    }

    private void ChunkVoxelRes(ChunkResVoxMessage msg)
    {
        List<NeighborReq> list;
        if (!chunkRequests.TryGetValue(msg.pos, out list))
        {
            Debug.LogError("Failed to find a request to Chunk: " + msg.pos);
            return;
        }

        if (!chunkRequests.Remove(msg.pos))
        {
            Debug.LogError("Failed to remove a request to Chunk: " + msg.pos);
            return;
        }

        VoxRef voxRef = new VoxRef(_sharedData.buffer);

        if (msg.list == null)
        {
            //If the list was null, this means the chunk doesn't exists or is empty, so set all voxels visible.
            foreach (NeighborReq req in list)
            {
                voxRef.Target(req.vox);
                voxRef.SetVisible(req.side, true);
            }
        }
        else
        {
            foreach (KeyValuePair<Vec3, VoxSnap> pair in msg.list)
            {
                NeighborReq req = list.Find((r) => r.neighbor == pair.Key);
                voxRef.Target(req.vox);
                voxRef.SetVisible(req.side, (pair.Value == null || pair.Value.IsEmpty()));
            }
        }

        if (chunkRequests.Count == 0)
        {
            Finish();
        }
    }
}