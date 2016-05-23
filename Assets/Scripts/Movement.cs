using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.W)) transform.position = transform.position + Vector3.forward;
		if(Input.GetKeyDown(KeyCode.S)) transform.position = transform.position + Vector3.back;
		if(Input.GetKeyDown(KeyCode.A)) transform.position = transform.position + Vector3.left;
		if(Input.GetKeyDown(KeyCode.D)) transform.position = transform.position + Vector3.right;
	}
}
