using UnityEngine;
using System.Collections;

public class Chunk
{
    public static readonly int SIZE = 16;

    private ChunkController _controller;
    private Vec3 _pos;
    private ChunkBuffer _buffer;

    public Vec3[] _neighbors;

    private ushort _voxelCount;

    public Chunk(ChunkController controller, Vec3 pos)
    {
        _controller = controller;
        _pos = pos;

        _buffer = new ChunkBuffer();
        _controller.Post(new ChunkMsg(_pos, ChunkMsg.Action.LOAD, null));
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

        _controller.Post(_pos, ChunkMsg.Action.SETUP, null);
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
                _controller.Post(_pos, ChunkMsg.Action.DETACH, null);
            }
        }
        else
        {
            CheckVisibleFaces();
            _controller.Post(_pos, ChunkMsg.Action.BUILD, null);
        }
    }

    public void Build()
    {
        var builder = new FacesMerger(_buffer).Merge();
        
        _controller.Post(_pos, ChunkMsg.Action.ATTACH, builder.PrebuildMesh());
    }

    private void CheckVisibleFaces()
    {
        VoxRef voxRef = new VoxRef(_buffer);
        VoxRef ngborVoxRef = new VoxRef(_buffer);
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
                            // Chunk neighborChunk = _neighbors[side];

                            // if (neighborChunk != null && !neighborChunk.IsEmpty() && neighborChunk.TryGetVox(neighborPos % Chunk.SIZE, neighborRef))
                            // {
                            //     voxRef.SetVisible(side, neighborRef.IsEmpty());
                            // }
                            // else
                            // {
                            voxRef.SetVisible(side, true);
                            // }
                        }
                    }
                }
            }
        }
    }
}