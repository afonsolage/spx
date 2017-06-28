using UnityEngine;
using System.Collections.Generic;

public class ChunkMeshBuilder
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
        public readonly short type;
        public readonly List<float> buffer = new List<float>();

        public Data(byte side, short type) { this.side = side; this.type = type; }
    }

    private List<Data> dataList;

    private Data Get(short type, byte side)
    {
        return dataList.Find((d) => d.side == side && d.type == type);
    }

    private Data SafeGet(short type, byte side)
    {
        var data = Get(type, side);

        if (data == null)
        {
            data = new Data(side, type);
            dataList.Add(data);
        }

        return data;
    }

    public void Add(short type, byte side, float[] buffer)
    {
        var data = SafeGet(type, side);
        data.buffer.AddRange(buffer);
    }

    private int GetPositionCount()
    {
        int cnt = 0;

        dataList.ForEach((d) => cnt += d.buffer.Count);

        return cnt;
    }

    public float[] GetPositions()
    {
        List<float> result = new List<float>(GetPositionCount());
        dataList.ForEach((d) => result.AddRange(d.buffer));
        return result.ToArray();
    }

    public float[] GetNormals()
    {
        List<float> result = new List<float>(GetPositionCount());
        dataList.ForEach((d) => result.AddRange(NORMALS[d.side]));
        return result.ToArray();
    }

    public int[] GetIndexes()
    {
        //Each side has 4 vertex, with 3 floats each which makes 12 floats.
        //We need 6 index for each side, so need the half size.
        int indexCount = (int)(GetPositionCount() / 2);
        List<int> result = new List<int>(indexCount);

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

        for (int i = 0; i < indexCount; i++)
        {
            result[i++] = n; //0
            result[i++] = n + 1; //1
            result[i++] = n + 2; //2
            result[i++] = n + 2;   //2
            result[i++] = n + 3; //3
            result[i++] = n; //0

            n += 4;
        }

        return result.ToArray();
    }

	public float[] GetUVs()
	{
		//A vertex is made of 3 floats.
		int vertexCount = (int)(GetPositionCount() /3);
		List<float> result = new List<float>(vertexCount);

		        float x1, y1, z1;
        float x2, y2, z2;
        float x4, y4, z4;

        float xTile;
        float yTile;
        int j = 0;
        foreach (Data data in dataList) {
            for (int i = 0, size = data.buffer.Count; i < size;) {
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

                result[j++] = xTile;
                result[j++] = 0f;
                result[j++] = 0f;
                result[j++] = 0f;
                result[j++] = 0f;
                result[j++] = yTile;
                result[j++] = xTile;
                result[j++] = yTile;
            }
        }

		return result.ToArray();
	}
}