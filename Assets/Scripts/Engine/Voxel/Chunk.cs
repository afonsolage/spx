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
    public enum PipelineStage
    {
        CREATED,
        LOADED,
        SETUP,
        LIGHT_PREPARED,
        LIGHT_SMOOTHED,
        BUILT,
        UNLOADED,
    }

    public static readonly int SIZE = 16;

    private WeakReference _controller;
    private Vec3 _pos;
    private ChunkBuffer _buffer;

    public Vec3[] _neighbors;

    private ushort _voxelCount;

    private PipelineStage _stage;

    private List<ChunkVoxCallback> _chunkVoxCBList;
    private bool _attached;
    private ChunkLighting _lighting;

    public Chunk(ChunkController controller, Vec3 pos)
    {
        _controller = new WeakReference(controller);
        _pos = pos;
        _chunkVoxCBList = new List<ChunkVoxCallback>();
        _buffer = new ChunkBuffer();

        _stage = PipelineStage.CREATED;
        GotoNextStage();
    }

    private Vec3 position
    {
        get
        {
            return _pos;
        }
    }

    private bool IsEmpty()
    {
        return _voxelCount == 0;
    }

    private bool TryGetVox(Vec3 pos, VoxRef voxRef)
    {
        voxRef.Bind(_buffer);
        return voxRef.TryTarget(pos);
    }

    private void GotoNextStage(object param = null)
    {
        switch (_stage)
        {
            case PipelineStage.CREATED:
                GetController()?.Post(_pos, ChunkAction.LOAD);
                break;
            case PipelineStage.LOADED:
                GetController()?.Post(_pos, ChunkAction.SETUP);
                break;
            case PipelineStage.SETUP:
                GetController()?.Post(_pos, ChunkAction.LIGHT_PREPARE);
                break;
            case PipelineStage.LIGHT_PREPARED:
                GetController()?.Post(_pos, ChunkAction.LIGHT_SMOOTH);
                break;
            case PipelineStage.LIGHT_SMOOTHED:
                GetController()?.Post(_pos, ChunkAction.BUILD);
                break;
            case PipelineStage.BUILT:
                _attached = true;
                GetController()?.Post(new ChunkAttachMessage(_pos, param as PrebuiltMesh));
                break;
            case PipelineStage.UNLOADED:
                _attached = false;
                GetController()?.Post(_pos, ChunkAction.DETACH);
                break;
            default:
                Debug.LogError("Invalid pipeline stage: " + _stage);
                break;
        }
    }

    private ChunkController GetController()
    {
        return _controller.Target as ChunkController;
    }

    public void Dispatch(ChunkMessage msg)
    {
        switch (msg.action)
        {
            case ChunkAction.NONE:
                //do nothing.
                break;
            case ChunkAction.LOAD:
                Load();
                break;
            case ChunkAction.SETUP:
                Setup();
                break;
            case ChunkAction.LIGHT_PREPARE:
                PrepareLight();
                break;
            case ChunkAction.LIGHT_SMOOTH:
                _lighting?.ComputeSmoothLighting();
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
            case ChunkAction.REQ_SUNLIGHT:
                SunlightRequest(msg as ChunkReqSunlightMessage);
                break;
            case ChunkAction.RES_SUNLIGHT:
                SunlightResponse(msg as ChunkResSunlightMessage);
                break;
            default:
                Debug.LogWarning("Unsupported action received: " + msg.action);
                break;
        }
    }

    private void Load()
    {
        if (_stage == PipelineStage.LOADED)
            return;

        _buffer.Allocate();

        _neighbors = new Vec3[Vec3.ALL_DIRECTIONS.Length];
        int i = 0;
        foreach (Vec3 dir in Vec3.ALL_DIRECTIONS)
        {
            _neighbors[i++] = _pos + dir * Chunk.SIZE;
        }

        _stage = PipelineStage.LOADED;
        GotoNextStage();
    }

    public void Unload()
    {
        if (_buffer.IsAllocated())
            _buffer.Free();

        _neighbors = null;
        _voxelCount = 0;
        _chunkVoxCBList.Clear();
        _stage = PipelineStage.UNLOADED;

        if (_attached)
            GotoNextStage();
    }

    private void Setup()
    {
        if (_stage == PipelineStage.SETUP)
            return;

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
            Unload();
        }
        else
        {
            CheckVisibleFaces();
            //Request sunlight info to the upper Chunk.
            GetController()?.Post(new ChunkToChunkMessage(_pos, ChunkAction.REQ_SUNLIGHT, _neighbors[Voxel.TOP]));

            _stage = PipelineStage.SETUP;
        }
    }

    private void PrepareLight()
    {
        VoxRef vox = new VoxRef(_buffer);
        for (int x = 0; x < SIZE; x++)
        {
            for (int y = 0; y < SIZE; y++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    var pos = new Vec3(x, y, z);
                    vox.Target(pos);

                    if (vox.IsEmpty())
                        continue;

                    byte[] neighborLighting = new byte[ChunkLighting.CORNERS_DIR.Length];

                    int i = 0;
                    foreach (Vec3 dir in ChunkLighting.CORNERS_DIR)
                    {
                        var tgtVox = pos + dir;
                        if (vox.TryTarget(tgtVox))
                        {
                            var visible = vox.IsEmpty();
                            neighborLighting[i] = ChunkLighting.Calc(vox, visible);
                        }
                        else
                        {
                            var closureSide = i;
                            var closureNeighborLighting = neighborLighting;
                            ReqChunkVox(pos + (dir * SIZE), tgtVox % SIZE, (snap) =>
                            {
                                var visible = snap == null || snap.IsEmpty();
                                closureNeighborLighting[closureSide] = ChunkLighting.Calc(snap, visible);
                            });
                        }
                    }

                    _lighting.SetNeighborLighting(x, y, z, neighborLighting);
                }
            }
        }

        _stage = PipelineStage.LIGHT_PREPARED;
    }

    private void Build()
    {
        if (_stage == PipelineStage.BUILT)
            return;

        var builder = new FacesMerger(_buffer).Merge();

        _stage = PipelineStage.BUILT;
        GotoNextStage(builder.PrebuildMesh());
    }

    private void VoxRequest(ChunkReqVoxMessage msg)
    {
        VoxSnap snap = null;

        if (!IsEmpty())
        {
            var voxRef = new VoxRef(_buffer, msg.vox);
            if (voxRef.IsValid())
                snap = voxRef.Snapshot();
        }

        GetController()?.Post(new ChunkResVoxMessage(msg, snap));
    }

    private void VoxResponse(ChunkResVoxMessage msg)
    {
        var item = _chunkVoxCBList.Find(a => a.chunk == msg.pos && a.vox == msg.vox);

        if (item == null)
            return;

        _chunkVoxCBList.Remove(item);

        item.callback(msg.snap);

        if (_chunkVoxCBList.Count == 0)
        {
            GotoNextStage();
        }
    }

    private void ReqChunkVox(Vec3 chunk, Vec3 pos, Action<VoxSnap> action)
    {
        _chunkVoxCBList.Add(new ChunkVoxCallback(chunk, pos, action));
        GetController()?.Post(new ChunkReqVoxMessage(_pos, chunk, pos));
    }

    private void CheckVisibleFaces()
    {
        VoxRef voxRef = new VoxRef(_buffer);
        VoxRef ngborVoxRef = new VoxRef(_buffer);

        _lighting = new ChunkLighting();

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

                    var sideLighting = new byte[Voxel.ALL_SIDES.Length];
                    foreach (byte side in Voxel.ALL_SIDES)
                    {
                        var neighborPos = voxRef.SideDir(side) + voxRef.GetPos();

                        if (ngborVoxRef.TryTarget(neighborPos))
                        {
                            var visible = ngborVoxRef.IsEmpty();
                            voxRef.SetVisible(side, visible);
                            sideLighting[side] = ChunkLighting.Calc(voxRef, visible);
                        }
                        else
                        {
                            //Create a variable here to be captured by following closure.
                            var closureVoxRef = voxRef.Clone();
                            var closureSideLighting = sideLighting;
                            var closureSide = side;
                            ReqChunkVox(_neighbors[side], neighborPos % Chunk.SIZE, (snap) =>
                            {
                                var visible = snap == null || snap.IsEmpty();
                                closureVoxRef.SetVisible(side, visible);
                                closureSideLighting[closureSide] = ChunkLighting.Calc(snap, visible);
                            });
                        }
                    }

                    _lighting.SetSidesLightign(x, y, z, sideLighting);
                }
            }
        }
    }

    private void SunlightRequest(ChunkReqSunlightMessage msg)
    {
        byte[,] data = null;

        if (!IsEmpty())
        {
            data = new byte[SIZE, SIZE];
            VoxRef vox = new VoxRef(_buffer);
            for (int x = 0; x < SIZE; x++)
            {
                for (int z = 0; z < SIZE; z++)
                {
                    vox.Target(x, 0, z);
                    data[x, z] = vox.sunLight;
                }
            }
        }

        GetController()?.Post(new ChunkResSunlightMessage(msg, data));
    }

    private void SunlightResponse(ChunkResSunlightMessage msg)
    {
        var data = msg.data;

        Queue<Vec3> propagationQueue = new Queue<Vec3>();

        int y = Chunk.SIZE - 1;
        VoxRef vox = new VoxRef(_buffer);
        for (int x = 0; x < SIZE; x++)
        {
            for (int z = 0; z < SIZE; z++)
            {
                if (data == null || data[x, z] == Voxel.SUNLIGHT_MAX_VALUE)
                {
                    vox.Target(x, y, z);
                    if (vox.type == Voxel.VT_EMPTY)
                    {
                        vox.sunLight = Voxel.SUNLIGHT_MAX_VALUE;
                        propagationQueue.Enqueue(vox.GetPos() - Vec3.BOTTOM);
                    }
                }
            }
        }

        while (propagationQueue.Count > 0)
        {
            var pos = propagationQueue.Dequeue();

            if (vox.TryTarget(pos) && vox.type == Voxel.VT_EMPTY)
            {
                vox.sunLight = Voxel.SUNLIGHT_MAX_VALUE;
                propagationQueue.Enqueue(vox.GetPos() - Vec3.BOTTOM);
            }
        }

        GotoNextStage();
    }
}