using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLightData
{
    public readonly Vec3 voxel;
    public readonly float[][] vertexLight;

    public ChunkLightData(Vec3 voxel)
    {
        this.voxel = voxel;
        this.vertexLight = new float[Voxel.ALL_SIDES.Length][];
    }
}

public class ChunkLightingStage : ChunkBaseStage
{
    class NeighborReq
    {
        public readonly byte[] neighborLighting;
        public readonly int side;
        public readonly Vec3 neighbor;

        public NeighborReq(byte[] neighborLighting, Vec3 neighbor, int side)
        {
            this.neighborLighting = neighborLighting;
            this.neighbor = neighbor;
            this.side = side;
        }
    }

    private static readonly byte INVALID_LIGHTING = 255;

    /*
        -               +-------++-------++-----+
        -             /   8   //  5    //  2   /|
        -            +-------++-------++------+ |
        -          /   7   //   4   //   1   /| +
        -         +-------++-------++-------+ |/+
        -       /   6   //    3   //   0   /| +
        -       +-------++-------++-------+ |/
        -       |       ||       ||       | +
        -       |       ||       ||       |/
        -       +-------++-------++-------+
        -               +-------++-------++-----+
        -             /  16    //  13   // 11  /|
        -            +-------++-------++------+ |
        -          /   15  / ||       /  10  /| +
        -         +-------++-------++-------+ |/
        -        /  14   //  12   //   9   /| +
        -       +-------++-------++-------+ |/ 
        -       |       ||       ||       | +
        -       |       ||       ||       |/ 
        -       +-------++-------++-------+ 
        -               +-------++-------++-----+
        -             /  25   //   22   // 19  /|
        -            +-------++-------++------+ |
        -          /   24  //   21   // 18   /| +
        -         +-------++-------++-------+ |/
        -        /  23   //  20   //  17   /| +
        -       +-------++-------++-------+ |/
        -       |       ||       ||       | +
        -       |       ||       ||       |/
        -       +-------++-------++-------+
        Y
        | 
        | 
        |
        +------X
       /
      /
     Z
     
    */
    private static readonly int[,,] CORNERS =
    {
        {{20, 14, 23}, {20,  9, 17}, { 3,  9,  0}, { 3, 14,  6}}, //Front
        {{18,  9, 17}, {18, 11, 19}, { 1, 11,  2}, { 1,  9,  0}}, //Right
        {{22, 11, 19}, {22, 16, 25}, { 5, 16,  8}, { 5, 11,  2}}, //Back
        {{24, 16, 25}, {24, 14, 23}, { 7, 14,  6}, { 7, 16,  8}}, //Left
        {{ 3,  7,  6}, { 3,  1,  0}, { 5,  1,  2}, { 5,  7,  8}}, //Top
        {{22, 24, 25}, {22, 18, 19}, {20, 18, 17}, {20, 24, 23}}, //Down
    };
    private static readonly int[] SIDES = { 12, 10, 13, 15, 4, 21 };

    private byte[,,][] _verticesLightings;
    Dictionary<Vec3, List<NeighborReq>> _requests = new Dictionary<Vec3, List<NeighborReq>>();

    public ChunkLightingStage(SharedData sharedData) : base(ChunkStage.LIGHTING, sharedData)
    {
        _verticesLightings = new byte[Chunk.SIZE, Chunk.SIZE, Chunk.SIZE][];
    }

    protected override void OnStart()
    {
        VoxRef vox = new VoxRef(_sharedData.buffer);
        VoxRef ngborVox = new VoxRef(_sharedData.buffer);

        for (int x = 0; x < Chunk.SIZE; x++)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    vox.Target(x, y, z);

                    if (vox.IsEmpty())
                        continue;

                    var pos = vox.GetPos();
                    var neighborLighting = new byte[Vec3.ALL_DIRS.Length];

                    for (int i = 0; i < Vec3.ALL_DIRS.Length; i++)
                    {
                        var dir = Vec3.ALL_DIRS[i];

                        //If this is a Unit Direction (FORWARD, RIGHT, BACKWARD, LEFT, UP, BOTTOM)
                        if (dir.IsUnit())
                        {
                            byte side = (byte)Array.IndexOf(SIDES, i); //Get it's corresponding side
                            if (!vox.IsVisible(side))
                            {
                                //We don't need to check for neighbor if it isn't visible.
                                neighborLighting[i] = INVALID_LIGHTING;
                                continue;
                            }
                        }

                        var ngborPos = pos + dir;

                        if (ngborVox.TryTarget(ngborPos))
                        {
                            neighborLighting[i] = ngborVox.IsEmpty() ? INVALID_LIGHTING : ngborVox.light;
                        }
                        else
                        {
                            var ngborChunk = _sharedData.pos + dir * Chunk.SIZE;
                            ngborPos %= Chunk.SIZE;
                            AddRequestChunkVoxel(neighborLighting, ngborChunk, ngborPos, i);
                        }
                    }

                    _verticesLightings[x, y, z] = neighborLighting;
                }
            }
        }

        if(_requests.Count > 0)
            SendRequest();
        else
            Finish();
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

    private void AddRequestChunkVoxel(byte[] neighborLighting, Vec3 targetChunk, Vec3 targetVox, int side)
    {
        List<NeighborReq> list;
        if (!_requests.TryGetValue(targetChunk, out list))
        {
            list = new List<NeighborReq>();
            _requests[targetChunk] = list;
        }
        list.Add(new NeighborReq(neighborLighting, targetVox, side));
    }

    private void SendRequest()
    {
        foreach (KeyValuePair<Vec3, List<NeighborReq>> pair in _requests)
        {
            List<Vec3> list = new List<Vec3>();
            pair.Value.ForEach((r) => list.Add(r.neighbor));
            _sharedData.controller.Post(new ChunkReqVoxMessage(_sharedData.pos, pair.Key, list));
        }
    }

    private void ChunkVoxelRes(ChunkResVoxMessage msg)
    {
        List<NeighborReq> list;
        if (!_requests.TryGetValue(msg.pos, out list))
        {
            Debug.Log("Failed to find a request to Chunk: " + msg.pos);
            return;
        }

        _requests.Remove(msg.pos);

        if (msg.list == null)
        {
            //If the list was null, this means the chunk doesn't exists or is empty, so set lighting to invalid.
            foreach (NeighborReq req in list)
            {
                req.neighborLighting[req.side] = INVALID_LIGHTING;
            }
        }
        else
        {
            foreach (KeyValuePair<Vec3, VoxSnap> pair in msg.list)
            {
                NeighborReq req = list.Find((r) => r.neighbor == pair.Key);

                req.neighborLighting[req.side] = pair.Value?.light ?? INVALID_LIGHTING;
            }
        }

        if (_requests.Count == 0)
        {
            ComputeSmoothLighting();
            Finish();
        }
    }

    private void ComputeSmoothLighting()
    {
        //Each voxel has 6 sides and 4 vertex.
        List<ChunkLightData> vertLights = new List<ChunkLightData>(_sharedData.voxelCount * 6);

        for (int x = 0; x < Chunk.SIZE; x++)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    var verts = _verticesLightings[x, y, z];

                    if (verts == null)
                        continue;

                    var data = new ChunkLightData(new Vec3(x, y, z));
                    foreach (byte side in Voxel.ALL_SIDES)
                    {
                        int sideIdx = SIDES[side];

                        if (verts[sideIdx] == INVALID_LIGHTING)
                            continue;

                        float[] vertices = new float[4];

                        for (int i = 0; i < 4; i++)
                        {
                            vertices[i] = AmbientOcclusion(verts[sideIdx], verts[CORNERS[side, i, 0]], verts[CORNERS[side, i, 1]], verts[CORNERS[side, i, 2]]);
                        }

                        data.vertexLight[side] = vertices;
                    }
                    vertLights.Add(data);

                    //For debug, to ensure there is no sharing reference.
                    _verticesLightings[x, y, z] = null;
                }
            }
        }

        this._output = vertLights;
    }

    /**
	 * Compute the smooth lighting value based of both sides and the corner. This algorithm was based on this blog post:
	 * https://0fps.net/2013/07/03/ambient-occlusion-for-minecraft-like-worlds/
	 * 
	 * @param sideLighting
	 *            The lighting value on this voxel side.
	 * @param side1
	 *            The lighting value of first side of this voxel.
	 * @param side2
	 *            The lighting value of second side of this voxel.
	 * @param corner
	 *            The lighting value of corner of this voxel.
	 * @return The final smoothed light value.
	 */
    private static float AmbientOcclusion(int sideLighting, byte side1, byte side2, byte corner)
    {
        ParseValue(ref side1);
        ParseValue(ref side2);
        ParseValue(ref corner);

        // if side1 and side2 have no light, corner should be darker also, because side1 and side2
        // are blocking corner light.
        if (side1 == 0 && side2 == 0)
        {
            corner = 0;
        }
        return (sideLighting + side1 + side2 + corner) / 4f;
    }

    private static void ParseValue(ref byte val)
    {
        if (val == INVALID_LIGHTING)
            val = 0;
    }
}

