using UnityEngine;
using System.Collections.Generic;

public class MeshBuilder
{
    private static readonly float[][] NORMALS =
    {
        new float[]{0.0f, 0.0f, 1.0f}, //FRONT
		new float[]{1.0f, 0.0f, 0.0f}, //RIGHT
		new float[]{0.0f, 0.0f, -1.0f}, //BACK
		new float[]{-1.0f, 0.0f, 0.0f}, //LEFT
		new float[]{0.0f, 1.0f, 0.0f}, //TOP
		new float[]{0.0f, -1.0f, 0.0f}, //DOWN
	};

    class Data
    {
        public readonly byte side;
        public readonly ushort type;
        public readonly List<float> buffer = new List<float>();

        public Data(byte side, ushort type) { this.side = side; this.type = type; }
    }

    private List<Data> dataList = new List<Data>();

    private Data Get(ushort type, byte side)
    {
        return dataList.Find((d) => d.side == side && d.type == type);
    }

    private Data SafeGet(ushort type, byte side)
    {
        var data = Get(type, side);

        if (data == null)
        {
            data = new Data(side, type);
            dataList.Add(data);
        }

        return data;
    }

    public void Add(ushort type, byte side, float[] buffer)
    {
        var data = SafeGet(type, side);
        data.buffer.AddRange(buffer);
    }

    private int GetPositionFloatCount()
    {
        int cnt = 0;

        dataList.ForEach((d) => cnt += d.buffer.Count);

        return cnt;
    }

    private int GetPositionCount()
    {
        return GetPositionFloatCount() / 3;
    }

    private void ParseFloatBuffer(List<Vector3> output, List<float> input)
    {
        Debug.Assert(input.Count % 3 == 0);

        for (int i = 0; i < input.Count; i += 3)
            output.Add(new Vector3(input[i], input[i + 1], input[i + 2]));

    }

    private void ParseFloatBuffer(List<Vector3> output, float[] input)
    {
        Debug.Assert(input.Length % 3 == 0);

        for (int i = 0; i < input.Length; i += 3)
            output.Add(new Vector3(input[i], input[i + 1], input[i + 2]));

    }

    public List<Vector3> GetPositions()
    {
        List<Vector3> result = new List<Vector3>(GetPositionCount());
        dataList.ForEach((d) => ParseFloatBuffer(result, d.buffer));
        return result;
    }

    public List<Vector3> GetNormals()
    {
        List<Vector3> result = new List<Vector3>(GetPositionCount());
        dataList.ForEach((d) =>
        {
            //For each vertex, add a normal to the givin side.
            for (int i = 0; i < d.buffer.Count; i += 3)
                ParseFloatBuffer(result, NORMALS[d.side]);
        });
        return result;
    }

    public int[] GetIndices()
    {
        //Each side has 4 vertex, with 3 floats each which makes 12 floats.
        //We need 6 index for each side, so need the half size.
        int indexCount = (int)(GetPositionFloatCount() / 2);
        int[] result = new int[indexCount];

        int n = 0;
        /*  Vertexes are built using the counter-clockwise, we just need to follow this index pattern:
         *		     3		   2    2
         *		     +--------+    + 
         *		     |       /   / |
         *		     |     /   /   |
         *		     |   /   /     |
         *		     | /   /	   |
         *		     +   +---------+
         *		    0    0	        1
         */

        for (int i = 0; i < indexCount;)
        {
            result[i++] = n; //0
            result[i++] = n + 1; //1
            result[i++] = n + 2; //2
            result[i++] = n + 2;   //2
            result[i++] = n + 3; //3
            result[i++] = n; //0

            n += 4;
        }

        return result;
    }

    public List<Vector2> GetUVs()
    {
        //A vertex is made of 3 floats.
        List<Vector2> result = new List<Vector2>(GetPositionCount());

        float x1, y1, z1;
        float x2, y2, z2;
        float x4, y4, z4;

        float xTile;
        float yTile;
        foreach (Data data in dataList)
        {
            for (int i = 0, j = 0, size = data.buffer.Count; i < size; j += 8)
            {
                x1 = data.buffer[i++];
                y1 = data.buffer[i++];
                z1 = data.buffer[i++];

                x2 = data.buffer[i++];
                y2 = data.buffer[i++];
                z2 = data.buffer[i++];

                //skip v3
                i += 3;

                x4 = data.buffer[i++];
                y4 = data.buffer[i++];
                z4 = data.buffer[i++];

                xTile = Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) + Mathf.Abs(z1 - z2);
                yTile = Mathf.Abs(x1 - x4) + Mathf.Abs(y1 - y4) + Mathf.Abs(z1 - z4);

                result.Add(new Vector2(xTile, 0f));
                result.Add(new Vector2(0f, 0f));
                result.Add(new Vector2(0f, yTile));
                result.Add(new Vector2(xTile, yTile));
            }
        }

        return result;
    }

    public List<Vector2> getTileUVs()
    {
        //A vertex is made of 3 floats.
        List<Vector2> result = new List<Vector2>(GetPositionCount());

        foreach (Data data in dataList)
        {
            for (int i = 0, size = data.buffer.Count; i < size;)
            {
                result.Add(new Vector2(0.0f, 1.0f)); //TODO: Add tiling conf on proper place.
                i += 3;
            }
        }

        return result;
    }

    public List<Color> getColors()
    {
        List<Color> result = new List<Color>(GetPositionCount());

        foreach (Data data in dataList)
        {
            for (int i = 0, size = data.buffer.Count; i < size;)
            {
                result.Add(Color.white); //TODO: Add tiling conf on proper place.
                i += 3;
            }
        }

        return result;
    }
}