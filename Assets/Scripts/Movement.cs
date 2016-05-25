using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour {

	private Rigidbody _rb;
	public float speed;

	// Use this for initialization
	void Start () {
		_rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.W)) _rb.AddForce(transform.forward*speed);
		if(Input.GetKey(KeyCode.S)) _rb.AddForce(-transform.forward*speed);
		if(Input.GetKey(KeyCode.A)) _rb.AddForce(-transform.right*speed);
		if(Input.GetKey(KeyCode.D)) _rb.AddForce(transform.right*speed);
	}
}
