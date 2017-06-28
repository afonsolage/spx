using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject go = new GameObject("Chunk Test");
		go.AddComponent<MeshFilter>();
		go.AddComponent<MeshRenderer>();
		go.AddComponent<Chunk>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
