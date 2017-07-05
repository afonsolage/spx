using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLightingStage : ChunkBaseStage
{
    class VertexLighting
    {
        public byte[] sidesLighting;
        public byte[] neighborLighting;
        public float[][] lighting;

        public VertexLighting(byte[] sidesLighting)
        {
            this.sidesLighting = sidesLighting;
        }
    }

    class NeighborReq
    {
        public readonly byte[] sideLighting;
        public readonly byte side;
        public readonly Vec3 neighbor;

        public NeighborReq(byte[] sideLighting, Vec3 neighbor, byte side)
        {
            this.sideLighting = sideLighting;
            this.neighbor = neighbor;
            this.side = side;
        }
    }

    private static readonly byte INVALID_LIGHTING = 255;

    private static readonly Vec3[] CORNERS_DIR = {
        /*
		-               +-------++-------++-----+
		-             /   7   //  4    //  2   /|
		-            +-------++-------++------+ |
		-          /   6   / ||      /   1   /| +
		-         +-------++-------++-------+ |/+
		-       /   5   //    3   //   0   /| +/|
		-       +-------++-------++-------+ |/+ |
		-       |       ||       ||       | + | +
		-       |       ||       ||       |/+ |/+
		-       +-------++-------++-------+/| +/|
		-       +-------+  +      +-------+ | + |
		-       |       | /+------|       | +/| +
		-       |       |//       |       |/+ |/ 
		-       +-------+/        +-------+/| +
		-       +-------++-------++-------+ |/ 
		-       |       ||       ||       | +  
		-       |       ||       ||       |/ 
		-       +-------++-------++-------+
		- Y
		- | 
		- | 
		- |
		- +------ X
		-  \
		-   \
		-    Z
        */
        //0                     1               2
        Vec3.RIGHT_UP_FORWARD,  Vec3.RIGHT_UP,  Vec3.RIGHT_UP_BACKWARD,
        //3                                     4
        Vec3.UP_FORWARD,                        Vec3.UP_BACKWARD,
        //5                     6               7
        Vec3.LEFT_UP_FORWARD,   Vec3.LEFT_UP,   Vec3.LEFT_UP_BACKWARD,

        /*
		-               +-------++-------++-----+
		-             /       //       //      /|
		-            +-------++-------++------+ |
		-          /       / ||      /       /| +
		-         +-------++-------++-------+ |/
		-       /       //        //       /| +
		-       +-------++-------++-------+ |/
		-       |       ||       ||       | +
		-       |       ||       ||       |/
		-       +-------++-------++-------+
		-
		-               +-------          +-----+
		-             /  11   /|        /   9  /|
		-            +-------+ |       +------+ |
		-            |       | +-------|      | +
		-         +-------+  |/     +-------+ |/+
		-       /  10    /|--++----/   8   /|-+/|
		-       +-------+ |/ ||   +-------+ | + |
		-       |       | |  ||   |       | +/| +
		-       |       |/+-------|       |/+ |/
		-       +-------+/        +-------+/| +
		-       +-------++-------++-------+ |/ 
		-       |       ||       ||       | +  
		-       |       ||       ||       |/ 
		-       +-------++-------++-------+
		- Y
		- | 
		- | 
		- |
		- +------ X
		-  \
		-   \
		-    Z
		*/
        //8                 9
        Vec3.RIGHT_FORWARD, Vec3.RIGHT_BACKWARD,
        //10                11
        Vec3.LEFT_FORWARD,  Vec3.LEFT_BACKWARD,

        /*
		-               +-------++-------++-----+
		-             /       //       //      /|
		-            +-------++-------++------+ |
		-          /       //||     //       /| +
		-         +-------++-------++-------+ |/+
		-       /       //        //       /| +/|
		-       +-------++-------++-------+ |/+ |
		-       |       ||       ||       | + | +
		-       |       ||       ||       |/+ |/
		-       +-------++-------++-------+/| +
		-       +-------+  +      +-------+ | 
		-       |       | /       |       | +  
		-       |       |/        |       |/ 
		-       +-------+         +-------+
		-
		-               +-------++-------++-----+
		-             /  19   //  16   //  14  /|
		-            +-------++-------++------+ |
		-          /  18   //||     //  13   /| +
		-         +-------++-------++-------+ |/
		-       /   17  //   15   //  12   /| +
		-       +-------++-------++-------+ |/
		-       |       ||       ||       | +
		-       |       ||       ||       |/
		-       +-------++-------++-------+
		- Y
		- | 
		- | 
		- |
		- +------ X
		-  \
		-   \
		-    Z
		*/

        //12                        13                  14
        Vec3.RIGHT_BOTTOM_FORWARD,  Vec3.RIGHT_BOTTOM,  Vec3.RIGHT_BOTTOM_BACKWARD,
        //15                                            16
        Vec3.BOTTOM_FORWARD,                            Vec3.BOTTOM_BACKWARD,
        //17                        18                  19
        Vec3.LEFT_BOTTOM_FORWARD,   Vec3.LEFT_BOTTOM,   Vec3.LEFT_BOTTOM_BACKWARD,
    };

    private static readonly int[,,] CORNERS =
    {
        {{15, 10, 17}, {15,  8, 12}, { 3,  8,  0}, { 3, 10,  5}}, //Front
        {{13,  8, 12}, {13,  9, 14}, { 1,  9,  2}, { 1,  8,  0}}, //Right
        {{16,  9, 14}, {16, 11, 19}, { 4, 11,  7}, { 4,  9,  2}}, //Back
        {{18, 11, 19}, {18, 10, 17}, { 6, 10,  5}, { 6, 11,  7}}, //Left
        {{ 3,  6,  5}, { 3,  1,  0}, { 4,  1,  2}, { 4,  6,  7}}, //Top
        {{16, 18, 19}, {16, 13, 14}, {15, 13, 12}, {15, 18, 17}}, //Down
    };

    private VertexLighting[,,] _verticesLightings;
    Dictionary<Vec3, List<NeighborReq>> _requests = new Dictionary<Vec3, List<NeighborReq>>();

    public ChunkLightingStage(SharedData sharedData) : base(ChunkStage.LIGHTING, sharedData)
    {
        _verticesLightings = new VertexLighting[Chunk.SIZE, Chunk.SIZE, Chunk.SIZE];
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

                    var sideLighting = new byte[Voxel.ALL_SIDES.Length];
                    foreach (byte side in Voxel.ALL_SIDES)
                    {
                        if (!vox.IsVisible(side))
                        {
                            sideLighting[side] = INVALID_LIGHTING;
                            continue;
                        }

                        var ngborPos = vox.SideNeighbor(side);

                        if (ngborVox.TryTarget(ngborPos))
                        {
                            sideLighting[side] = ngborVox.IsEmpty() ? INVALID_LIGHTING : ngborVox.light;
                        }
                        else
                        {
                            var ngborChunk = _sharedData.pos + Vec3.ALL_UNIT_DIRS[side] * Chunk.SIZE;
                            ngborPos %= Chunk.SIZE;
                            AddRequestChunkVoxel(sideLighting, ngborChunk, ngborPos, side);
                        }
                    }

                    SetSidesLighting(x, y, z, sideLighting);
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

    private void AddRequestChunkVoxel(byte[] sideLighting, Vec3 targetChunk, Vec3 targetVox, byte side)
    {
        List<NeighborReq> list;
        if (!_requests.TryGetValue(targetChunk, out list))
        {
            list = new List<NeighborReq>();
            _requests[targetChunk] = list;
        }
        list.Add(new NeighborReq(sideLighting, targetVox, side));
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
                req.sideLighting[req.side] = INVALID_LIGHTING;
            }
        }
        else
        {
            foreach (KeyValuePair<Vec3, VoxSnap> pair in msg.list)
            {
                NeighborReq req = list.Find((r) => r.neighbor == pair.Key);

                req.sideLighting[req.side] = pair.Value?.light ?? INVALID_LIGHTING;
            }
        }

        if (_requests.Count == 0)
        {
            ComputeSmoothLighting();
            Finish();
        }
    }

    private void SetSidesLighting(int x, int y, int z, byte[] data)
    {
        _verticesLightings[x, y, z] = new VertexLighting(data);
    }

    private void SetNeighborLighting(int x, int y, int z, byte[] neighborLighting)
    {
        _verticesLightings[x, y, z].neighborLighting = neighborLighting;
    }

    private void ComputeSmoothLighting()
    {
        for (int x = 0; x < Chunk.SIZE; x++)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    var vert = _verticesLightings[x, y, z];

                    if (vert == null)
                        continue;

                    vert.lighting = new float[Voxel.ALL_SIDES.Length][];

                    foreach (byte side in Voxel.ALL_SIDES)
                    {
                        if (vert.sidesLighting[side] == INVALID_LIGHTING)
                            continue;

                        float[] vertices = new float[4];

                        for (int i = 0; i < 4; i++)
                        {
                            vertices[i] = AmbientOcclusion(vert.sidesLighting[side], vert.neighborLighting[CORNERS[side, i, 0]], vert.neighborLighting[CORNERS[side, i, 1]], vert.neighborLighting[CORNERS[side, i, 2]]);
                        }

                        vert.lighting[side] = vertices;
                    }

                    //At this point, we don't need those infos anymore.
                    vert.sidesLighting = null;
                    vert.neighborLighting = null;
                }
            }
        }
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

