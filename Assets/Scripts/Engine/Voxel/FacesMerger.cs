public class FacesMerger
{
    private static readonly int SIDE_COUNT = Voxel.ALL_SIDES.Length;
    private ChunkBuffer buffer;
    private MeshBuilder builder;
    private byte[] mergedSides;

    public FacesMerger(ChunkBuffer buffer)
    {
        this.buffer = buffer;
        this.mergedSides = new byte[Chunk.SIZE * Chunk.SIZE * Chunk.SIZE * SIDE_COUNT];
        this.builder = new MeshBuilder();
    }

    private bool IsSideMerged(VoxRef v, byte side)
    {
        int offset = v.GetPos().ToVoxelOffset() * SIDE_COUNT;
        return mergedSides[offset + side] == 1;
    }

    private void SetSideMerged(VoxRef v, byte side)
    {
        int offset = v.GetPos().ToVoxelOffset() * SIDE_COUNT;
        mergedSides[offset + side] = 1;
    }

    /**
	 * Merge all voxels with a front (Z+) face of same type and visible. This methods uses a simple logic: It just
	 * iterate over X looking for neighbor voxels who can be merged (share the same type and have front face visible
	 * also). When it reaches the at right most voxel, it start looking for neigor at top (Y+) and repeat the proccess
	 * looking into right voxels until it reaches the right most and top most voxels, when it combine all those voxels
	 * on a singles float array and returns it.
	 * 
	 * @param buffer
	 *
	 */
    private void MergeFrontFaces()
    {
        // Voxel and Neightbor Voxel
        VoxRef v = new VoxRef(buffer);
        VoxRef nv = new VoxRef(buffer);

        // Merge origin x and y to keep track of merged face bounds.
        int ox, oy;
        bool done;
        ushort currentType;
        byte side = Voxel.FRONT;

        // When looking for front faces (Z+) we need to check X axis and later Y axis, because this, the iteration
        // occurs this way.
        for (int z = 0; z < Chunk.SIZE; z++)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int x = 0; x < Chunk.SIZE; x++)
                {
                    if (!v.TryTarget(x, y, z))
                    {
                        continue;
                    }

                    currentType = v.type;

                    // If voxel is invalid or is merged already, skip it;
                    if (currentType == Voxel.VT_EMPTY || IsSideMerged(v, side) || !v.IsVisible(side)/* || v.isSpecial()*/)
                    {
                        continue;
                    }

                    float[] vertices = new float[12];
                    ox = x;
                    oy = y;
                    done = false;

                    /*
					 * 
					 *      v7 +-------+ v6	y
					 *      / |      / |	|  
					 *   v3 +-------+v2|	|
					 *      |v4+-------+ v5	+-- X
					 *      | /     | /		 \
					 *      +-------+		  Z
					 *     v0        v1
					 */
                    // The front face is composed of v0, v1, v2 and v3.
                    System.Array.Copy(v.V0(), 0, vertices, 0, 3);

                    /* 
					 *	Following the counter-clockwise rendering order.
					 * 
					 *	    |
					 *	    +
					 *	    |		    --->
					 *	    +---+---
					 *	  v0			       
					 */
                    while (true)
                    {

                        // We are on the boundary of chunk. Stop it;
                        if (x == Chunk.SIZE - 1)
                        {
                            break;
                        }

                        // Move to the next voxel on X axis. If the next voxel is invalid
                        if (!nv.TryTarget(++x, y, z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                || (nv.type != currentType))
                        {
                            // Go back to previous voxel;
                            --x;
                            break;
                        }
                    }

                    /*
					 * 
					 *	    |		|
					 *	    +	    +
					 *	    |		|	    
					 *	    +---+---+---+
					 *	  v0		 v1	       
					 * 
					 *	At this moment, we reached the right most X, so set it as v1.
					 */
                    System.Array.Copy(VoxRef.V1(x, y, z), 0, vertices, 3, 3);

                    // Go one step on Y direction and repeat the previous logic.
                    while (!done)
                    {
                        if (y == Chunk.SIZE - 1)
                        {
                            // We are on the boundary of chunk. Stop it;
                            break;
                        }

                        y++;

                        for (int k = ox; k <= x; k++)
                        {
                            if (!nv.TryTarget(k, y, z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                    || (nv.type != currentType))
                            {
                                --y; // Go back to previous voxel;
                                done = true;
                                break;
                            }
                        }
                    }

                    /*	  v3		 v2
					 *	    +---+---+---+
					 *	    |		|
					 *	    +	        +
					 *	    |		|	    
					 *	    +---+---+---+
					 *	  v0		 v1	       
					 * 
					 *	At this moment, we reached the right most and top most, so lets track v2 and v3.
					 */

                    System.Array.Copy(VoxRef.V2(x, y, z), 0, vertices, 6, 3);
                    System.Array.Copy(VoxRef.V3(ox, y, z), 0, vertices, 9, 3);

                    for (int a = ox; a <= x; a++)
                    {
                        for (int b = oy; b <= y; b++)
                        {
                            if (v.TryTarget(a, b, z))
                            {
                                SetSideMerged(v, side);
                            }
                        }
                    }

                    builder.Add(currentType, side, vertices);
                    y = oy; // Set the y back to orignal location, so we can iterate over it again.
                }
            }
        }
    }

    private void MergeBackFaces()
    {
        // Voxel and Neightbor Voxel
        VoxRef v = new VoxRef(buffer);
        VoxRef nv = new VoxRef(buffer);

        // Merge origin x and y
        int ox, oy;
        bool done;
        ushort currentType;
        byte side = Voxel.BACK;

        for (int z = 0; z < Chunk.SIZE; z++)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int x = Chunk.SIZE - 1; x > -1; x--)
                {
                    if (!v.TryTarget(x, y, z))
                    {
                        continue;
                    }

                    currentType = v.type;

                    // If vox is invalid or is merged already, skip it;
                    if (currentType == Voxel.VT_EMPTY || IsSideMerged(v, side) || !v.IsVisible(side)/* || v.isSpecial()*/)
                    {
                        continue;
                    }

                    float[] vertices = new float[12];
                    ox = x;
                    oy = y;
                    done = false;

                    // The back face is composed of v5, v4, v7 and v6.
                    System.Array.Copy(v.V5(), 0, vertices, 0, 3);
                    while (true)
                    {
                        if (x == 0)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        // Move to the next voxel on X axis.
                        if (!v.TryTarget(--x, y, z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                || (nv.type != currentType))
                        {
                            ++x; // Go back to previous voxel;
                            break;
                        }

                        // v = nv; //Set current voxel as next one, so repeat the check until end.
                    }

                    System.Array.Copy(VoxRef.V4(x, y, z), 0, vertices, 3, 3);
                    while (!done)
                    {
                        if (y == Chunk.SIZE - 1)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        y++;

                        for (int k = ox; k >= x; k--)
                        {
                            if (!nv.TryTarget(k, y, z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                    || (nv.type != currentType))
                            {
                                --y; // Go back to previous voxel;
                                done = true;
                                break;
                            }
                        }
                    }

                    System.Array.Copy(VoxRef.V7(x, y, z), 0, vertices, 6, 3);
                    System.Array.Copy(VoxRef.V6(ox, y, z), 0, vertices, 9, 3);

                    for (int a = ox; a >= x; a--)
                    {
                        for (int b = oy; b <= y; b++)
                        {
                            if (v.TryTarget(a, b, z))
                            {
                                SetSideMerged(v, side);
                            }
                        }
                    }

                    builder.Add(currentType, side, vertices);
                    y = oy; // Set the y back to orignal location, so we can iterate over it again.
                }
            }
        }
    }

    private void MergeTopFaces()
    {
        // Voxel and Neightbor Voxel
        VoxRef v = new VoxRef(buffer);
        VoxRef nv = new VoxRef(buffer);

        // Merge origin x and y
        int ox, oz;
        bool done;
        ushort currentType;
        byte side = Voxel.TOP;

        for (int y = Chunk.SIZE - 1; y > -1; y--)
        {
            for (int z = Chunk.SIZE - 1; z > -1; z--)
            {
                for (int x = 0; x < Chunk.SIZE; x++)
                {
                    if (!v.TryTarget(x, y, z))
                    {
                        continue;
                    }

                    currentType = v.type;

                    // If vox is invalid or is merged already, skip it;
                    if (currentType == Voxel.VT_EMPTY || IsSideMerged(v, side) || !v.IsVisible(side)/* || v.isSpecial()*/)
                    {
                        continue;
                    }

                    float[] vertices = new float[12];
                    ox = x;
                    oz = z;
                    done = false;

                    // The top face is composed of v3, v2, v6 and v7.
                    System.Array.Copy(v.V3(), 0, vertices, 0, 3);
                    while (true)
                    {
                        if (x == Chunk.SIZE - 1)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        // Move to the next voxel on X axis.
                        if (!nv.TryTarget(++x, y, z) || nv.type == Voxel.VT_EMPTY || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                || (nv.type != currentType))
                        {
                            --x; // Go back to previous voxel;
                            break;
                        }

                        // v = nv; //Set current voxel as next one, so repeat the check until end.
                    }

                    System.Array.Copy(VoxRef.V2(x, y, z), 0, vertices, 3, 3);

                    while (!done)
                    {
                        if (z == 0)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        z--;

                        for (int k = ox; k <= x; k++)
                        {
                            if (!nv.TryTarget(k, y, z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                    || (nv.type != currentType))/* || v.isSpecial()*/
                            {
                                ++z; // Go back to previous voxel;
                                done = true;
                                break;
                            }
                        }
                    }

                    System.Array.Copy(VoxRef.V6(x, y, z), 0, vertices, 6, 3);
                    System.Array.Copy(VoxRef.V7(ox, y, z), 0, vertices, 9, 3);

                    for (int a = ox; a <= x; a++)
                    {
                        for (int b = oz; b >= z; b--)
                        {
                            if (v.TryTarget(a, y, b))
                            {
                                SetSideMerged(v, side);
                            }
                        }
                    }

                    builder.Add(currentType, side, vertices);
                    z = oz; // Set the z back to orignal location, so we can iterate over it again.
                }
            }
        }
    }

    private void MergeDownFaces()
    {
        // Voxel and Neightbor Voxel
        VoxRef v = new VoxRef(buffer);
        VoxRef nv = new VoxRef(buffer);

        // Merge origin x and y
        int ox, oz;
        bool done;
        ushort currentType;
        byte side = Voxel.DOWN;

        for (int y = 0; y < Chunk.SIZE; y++)
        {
            for (int z = 0; z < Chunk.SIZE; z++)
            {
                for (int x = 0; x < Chunk.SIZE; x++)
                {
                    if (!v.TryTarget(x, y, z))
                    {
                        continue;
                    }

                    currentType = v.type;

                    // If vox is invalid or is merged already, skip it;
                    if (currentType == Voxel.VT_EMPTY || IsSideMerged(v, side) || !v.IsVisible(side))
                    {
                        continue;
                    }

                    float[] vertices = new float[12];
                    ox = x;
                    oz = z;
                    done = false;

                    // The down face is composed of v4, v5, v1 and v0.
                    System.Array.Copy(v.V4(), 0, vertices, 0, 3);
                    while (true)
                    {
                        if (x == Chunk.SIZE - 1)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        // Move to the next voxel on X axis.
						if (!nv.TryTarget(++x, y, z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                || (nv.type != currentType))
                        {
                            --x; // Go back to previous voxel;
                            break;
                        }
                    }

					System.Array.Copy(VoxRef.V5(x, y, z), 0, vertices, 3, 3);
                    while (!done)
                    {
                        if (z == Chunk.SIZE - 1)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        z++;

                        for (int k = ox; k <= x; k++)
                        {
                            if (!nv.TryTarget(k, y, z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                    || (nv.type != currentType))
                            {
                                --z; // Go back to previous voxel;
                                done = true;
                                break;
                            }
                        }
                    }

                    System.Array.Copy(VoxRef.V1(x, y, z), 0, vertices, 6, 3);
                    System.Array.Copy(VoxRef.V0(ox, y, z), 0, vertices, 9, 3);

                    for (int a = ox; a <= x; a++)
                    {
                        for (int b = oz; b <= z; b++)
                        {
                            if (v.TryTarget(a, y, b))
                            {
                                SetSideMerged(v, side);
                            }
                        }
                    }

                    builder.Add(currentType, side, vertices);
                    z = oz; // Set the z back to orignal location, so we can iterate over it again.
                }
            }
        }
    }

    private void MergeRightFaces()
    {
        // Voxel and Neightbor Voxel
        VoxRef v = new VoxRef(buffer);
        VoxRef nv = new VoxRef(buffer);

        // Merge origin x and y
        int oz, oy;
        bool done;
        ushort currentType;
        byte side = Voxel.RIGHT;

        for (int x = Chunk.SIZE - 1; x > -1; x--)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int z = Chunk.SIZE - 1; z > -1; z--)
                {
                    if (!v.TryTarget(x, y, z))
                    {
                        continue;
                    }

                    currentType = v.type;

                    // If vox is invalid or is merged already, skip it;
                    if (currentType == Voxel.VT_EMPTY || IsSideMerged(v, side) || !v.IsVisible(side)/* || v.isSpecial()*/)
                    {
                        continue;
                    }

                    float[] vertices = new float[12];
                    oz = z;
                    oy = y;
                    done = false;

                    // The right face is composed of v1, v5, v6 and v2.
                    System.Array.Copy(v.V1(), 0, vertices, 0, 3);
                    while (true)
                    {
                        if (z == 0)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        // Move to the next voxel on X axis.
                        if (!nv.TryTarget(x, y, --z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                || (nv.type != currentType))
                        {
                            ++z; // Go back to previous voxel;
                            break;
                        }

                        // v = nv; //Set current voxel as next one, so repeat the check until end.
                    }

					System.Array.Copy(VoxRef.V5(x, y, z), 0, vertices, 3, 3);
                    while (!done)
                    {
                        if (y == Chunk.SIZE - 1)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        y++;

                        for (int k = oz; k >= z; k--)
                        {
                            if (!nv.TryTarget(x, y, k) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                    || (nv.type != currentType))
                            {
                                --y; // Go back to previous voxel;
                                done = true;
                                break;
                            }
                        }
                    }

					System.Array.Copy(VoxRef.V6(x, y, z), 0, vertices, 6, 3);
					System.Array.Copy(VoxRef.V2(x, y, oz), 0, vertices, 9, 3);

                    for (int a = oz; a >= z; a--)
                    {
                        for (int b = oy; b <= y; b++)
                        {
                            if (v.TryTarget(x, b, a))
                            {
                                SetSideMerged(v, side);
                            }
                        }
                    }

                    builder.Add(currentType, side, vertices);
                    y = oy; // Set the y back to orignal location, so we can iterate over it again.
                }
            }
        }
    }

    private void MergeLeftFaces()
    {
        // Voxel and Neightbor Voxel
        VoxRef v = new VoxRef(buffer);
        VoxRef nv = new VoxRef(buffer);

        // Merge origin x and y
        int oz, oy;
        bool done;
        ushort currentType;
        byte side = Voxel.LEFT;

        for (int x = 0; x < Chunk.SIZE; x++)
        {
            for (int y = 0; y < Chunk.SIZE; y++)
            {
                for (int z = 0; z < Chunk.SIZE; z++)
                {
                    if (!v.TryTarget(x, y, z))
                    {
                        continue;
                    }

                    currentType = v.type;

                    // If vox is invalid or is merged already, skip it;
                    if (currentType == Voxel.VT_EMPTY || IsSideMerged(v, side) || !v.IsVisible(side)/* || v.isSpecial()*/)
                    {
                        continue;
                    }

                    float[] vertices = new float[12];
                    oy = y;
                    oz = z;
                    done = false;

                    // The left face is composed of v4, v0, v3 and v7.
                    System.Array.Copy(v.V4(), 0, vertices, 0, 3);
                    while (true)
                    {
                        if (z == Chunk.SIZE - 1)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        // Move to the next voxel on Z axis.
                        if (!nv.TryTarget(x, y, ++z) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                || (nv.type != currentType))
                        {
                            --z; // Go back to previous voxel;
                            break;
                        }
                        // v = nv; //Set current voxel as next one, so repeat the check until end.
                    }

					System.Array.Copy(VoxRef.V0(x, y, z), 0, vertices, 3, 3);
                    while (!done)
                    {
                        if (y == Chunk.SIZE - 1)
                        {
                            break; // We are on the boundary of chunk. Stop it;
                        }

                        y++;

                        for (int k = oz; k <= z; k++)
                        {
                            if (!nv.TryTarget(x, y, k) || IsSideMerged(nv, side) || !nv.IsVisible(side)
                                    || (nv.type != currentType))
                            {
                                --y; // Go back to previous voxel;
                                done = true;
                                break;
                            }
                        }
                    }

					System.Array.Copy(VoxRef.V3(x, y, z), 0, vertices, 6, 3);
					System.Array.Copy(VoxRef.V7(x, y, oz), 0, vertices, 9, 3);

                    for (int a = oz; a <= z; a++)
                    {
                        for (int b = oy; b <= y; b++)
                        {
                            if (v.TryTarget(x, b, a))
                            {
                                SetSideMerged(v, side);
                            }
                        }
                    }

                    builder.Add(currentType, side, vertices);
                    y = oy; // Set the y back to orignal location, so we can iterate over it again.
                }
            }
        }
    }


    public MeshBuilder Merge()
    {
        MergeFrontFaces();
        MergeBackFaces();
		MergeTopFaces();
		MergeDownFaces();
		MergeRightFaces();
		MergeLeftFaces();

        return builder;
    }

}
