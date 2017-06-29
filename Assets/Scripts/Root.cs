using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject prefab = Resources.Load("Prefab/Controller/VoxelController") as GameObject;
		Instantiate(prefab);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
