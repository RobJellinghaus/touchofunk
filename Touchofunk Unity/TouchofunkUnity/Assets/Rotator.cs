using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {
    public bool Rotating;
    public bool Cloned;

	// Use this for initialization
	void Start ()
    {
        Rotating = true;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Rotating)
        {
            transform.Rotate(new Vector3(0f, 1f, 1f), 5f, Space.Self);
        }
	}
}
