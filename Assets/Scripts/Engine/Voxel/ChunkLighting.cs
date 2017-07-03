
using System;

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

public class ChunkLighting
{
    public static readonly byte INVALID_LIGHTING = 255;

    public static readonly Vec3[] CORNERS_DIR = {
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

        //RIGHT_TOP_FRONT   RIGHT_TOP_MID       RIGHT_TOP_BACK
        new Vec3(1, 1, 1),  new Vec3(1, 1, 0),  new Vec3(1, 1, -1),

        //MID_TOP_FRONT                         MID_TOP_BACK
        new Vec3(0, 1, 1),                      new Vec3(0, 1, -1),

        //LEFT_TOP_FRONT    LEFT_TOP_MID        LEFT_TOP_BACK
        new Vec3(-1, 1, 1), new Vec3(-1, 1, 0), new Vec3(-1, 1, -1),

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

        //RIGHT_MID_FRONT   RIGHT_MID_BACK
        new Vec3(1, 0, 1),  new Vec3(1, 0, -1),

        //LEFT_MID_FRONT    LEFT_MID_BACK
        new Vec3(-1, 0, 1), new Vec3(-1, 0, -1),

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
        //RIGHT_DOWN_FRONT   RIGHT_DOWN_MID         RIGHT_DOWN_BACK
        new Vec3(1, -1, 1),  new Vec3(1, -1, 0),    new Vec3(1, -1, -1),

        //MID_DOWN_FRONT                            MID_DOWN_BACK
        new Vec3(0, -1, 1),                         new Vec3(0, -1, -1),

        //LEFT_DOWN_FRONT    LEFT_DOWN_MID          LEFT_DOWN_BACK
        new Vec3(-1, -1, 1), new Vec3(-1, -1, 0),   new Vec3(-1, -1, -1),
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

    public static byte Calc(VoxRef vox, bool visible)
    {
        if (visible)
            return vox.light;
        else
            return INVALID_LIGHTING;
    }

    public static byte Calc(VoxSnap snap, bool visible)
    {
        if (visible)
            return snap?.light ?? Voxel.SUNLIGHT_MAX_VALUE;
        else
            return INVALID_LIGHTING;
    }

    public ChunkLighting()
    {
        _verticesLightings = new VertexLighting[Chunk.SIZE, Chunk.SIZE, Chunk.SIZE];
    }

    public void SetSidesLightign(int x, int y, int z, byte[] data)
    {
        _verticesLightings[x, y, z] = new VertexLighting(data);
    }

    public void SetNeighborLighting(int x, int y, int z, byte[] neighborLighting)
    {
        _verticesLightings[x, y, z].neighborLighting = neighborLighting;
    }

    public void ComputeSmoothLighting()
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