using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelController : MonoBehaviour
{
    public Material VoxelDiffuse;

    public void Start()
    {
		name = "Voxel Controller";
		
        GameObject go = new GameObject("Chunk Test");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>().materials = new Material[] { VoxelDiffuse };
        go.AddComponent<Chunk>();

		go.transform.parent = transform;
    }
}
