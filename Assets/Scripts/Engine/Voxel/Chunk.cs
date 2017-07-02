using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkVoxCallback
{
    public Vec3 chunk;
    public Vec3 vox;
    public Action<VoxSnap> callback;

    public ChunkVoxCallback(Vec3 c, Vec3 v, Action<VoxSnap> cb) { this.chunk = c; this.vox = v; this.callback = cb; }
}

public class Chunk
{
    public static readonly int SIZE = 16;

    private ChunkController _controller;
    private Vec3 _pos;
    private ChunkBuffer _buffer;

    public Vec3[] _neighbors;

    private ushort _voxelCount;

    private List<ChunkVoxCallback> _chunkVoxCBList;

    public Chunk(ChunkController controller, Vec3 pos)
    {
        _controller = controller;
        _pos = pos;
        _chunkVoxCBList = new List<ChunkVoxCallback>();

        _buffer = new ChunkBuffer();
        _controller.Post(new ChunkMessage(_pos, ChunkAction.LOAD));
    }

    public Vec3 position
    {
        get
        {
            return _pos;
        }
    }

    public bool IsEmpty()
    {
        return _voxelCount == 0;
    }

    public bool TryGetVox(Vec3 pos, VoxRef voxRef)
    {
        voxRef.Bind(_buffer);
        return voxRef.TryTarget(pos);
    }

    public void Dispatch(ChunkMessage msg)
    {
        switch (msg.action)
        {
            case ChunkAction.LOAD:
                Load();
                break;
            case ChunkAction.SETUP:
                Setup();
                break;
            case ChunkAction.BUILD:
                Build();
                break;
            case ChunkAction.REQ_VOX:
                VoxRequest(msg as ChunkReqVoxMessage);
                break;
            case ChunkAction.RES_VOX:
                VoxResponse(msg as ChunkResVoxMessage);
                break;
            default:
                Debug.LogWarning("Unsupported action received: " + msg.action);
                break;
        }
    }

    public void Load()
    {
        _buffer.Allocate();

        _neighbors = new Vec3[Vec3.ALL_DIRECTIONS.Length];

        Vec3[] items = new Vec3[_neighbors.Length];
        int i = 0;
        foreach (Vec3 dir in Vec3.ALL_DIRECTIONS)
        {
            items[i++] = _pos + dir * Chunk.SIZE;
        }

        _controller.Post(_pos, ChunkAction.SETUP);
    }

    public void Setup()
    {
        bool wasEmpty = IsEmpty();

        VoxRef voxRef = new VoxRef(_buffer, new Vec3());
        var wx = _pos.x;

        for (int x = 0; x < SIZE; x++, wx++)
        {
            var wz = _pos.z;
            for (int z = 0; z < SIZE; z++, wz++)
            {
                float height = (float)(MakeSomeNoise.Get(wx, 0, wz, 7 / 1000.0, 4, 0.4f) * SIZE);
                var wy = _pos.y;
                for (int y = 0; wy < height; y++, wy++)
                {
                    voxRef.Target(x, y, z);
                    voxRef.type = 1; // TODO: Add types

                    //If there is at least on block on this chunk, than itsn't empty.
                    _voxelCount++;
                }
            }
        }

        if (IsEmpty())
        {
            _buffer.Free();

            //If it wasn't empty before, we need detach it.
            if (!wasEmpty)
            {
                _controller.Post(_pos, ChunkAction.DETACH);
            }
        }
        else
        {
            CheckVisibleFaces();
            _controller.Post(_pos, ChunkAction.BUILD);
        }
    }

    public void Build()
    {
        var builder = new FacesMerger(_buffer).Merge();

        _controller.Post(new ChunkAttachMessage(_pos, builder.PrebuildMesh()));
    }

    private void VoxRequest(ChunkReqVoxMessage msg)
    {
        var voxRef = new VoxRef(_buffer, msg.vox);

        VoxSnap snap;
        if (voxRef.IsValid())
            snap = voxRef.Snapshot();
        else
            snap = null;

        _controller.Post(new ChunkResVoxMessage(msg, snap));
    }

    private void VoxResponse(ChunkResVoxMessage msg)
    {
        var item = _chunkVoxCBList.Find(a => a.chunk == msg.pos && a.vox == msg.vox);

        if (item == null)
            return;

        _chunkVoxCBList.Remove(item);

        item.callback(msg.snap);
    }

    private void ReqChunkVox(Vec3 chunk, Vec3 pos, Action<VoxSnap> action)
    {
        _chunkVoxCBList.Add(new ChunkVoxCallback(chunk, pos, action));
        _controller.Post(new ChunkReqVoxMessage(_pos, chunk, pos));
    }

    private void CheckVisibleFaces()
    {
        VoxRef voxRef = new VoxRef(_buffer);
        VoxRef ngborVoxRef = new VoxRef(_buffer);

        //We won't check borders now, since we'll need the neightboor info
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    voxRef.Target(x, y, z);

                    if (voxRef.IsEmpty())
                        continue;

                    foreach (byte side in Voxel.ALL_SIDES)
                    {
                        var neighborPos = voxRef.SideDir(side) + voxRef.GetPos();

                        if (ngborVoxRef.TryTarget(neighborPos))
                        {
                            voxRef.SetVisible(side, ngborVoxRef.IsEmpty());
                        }
                        else
                        {
                            //Create a variable here to be captured by following closure.
                            // var closureVoxRef = voxRef.Clone();
                            // ReqChunkVox(_neighbors[side], neighborPos % Chunk.SIZE, (snap) =>
                            // {
                            //     closureVoxRef.SetVisible(side, snap.IsEmpty());
                            // });
                        }
                    }
                }
            }
        }
    }
}