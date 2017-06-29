using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelController : MonoBehaviour
{
    public Material voxelDiffuse;

    public void Start()
    {
		name = "Voxel Controller";
		
        GameObject go = new GameObject("Chunk Test");
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.AddComponent<Chunk>().SetDiffuseMaterial(voxelDiffuse);

		go.transform.parent = transform;
    }
}
