using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject prefab = Resources.Load("Prefab/Voxel/VoxelViewport") as GameObject;
		Instantiate(prefab);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
