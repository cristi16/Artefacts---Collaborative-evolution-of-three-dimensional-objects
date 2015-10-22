using UnityEngine;
using System.Collections;

public class MoveAround : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.T))
	    {
	        GetComponent<Rigidbody>().AddForce(Vector3.up * 10f, ForceMode.Impulse);
	    }
	}
}
