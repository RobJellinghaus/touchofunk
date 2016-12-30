/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Collider collider = hit.collider;
                Rotator rotator = (Rotator)collider.GetComponentInChildren(typeof(Rotator));
                if (rotator != null)
                {
                    rotator.Rotating = !((Rotator)rotator).Rotating;

                    if (rotator.Rotating && !rotator.Cloned)
                    {
                        Instantiate(collider, collider.transform.position + new Vector3(2f, 0f, 0f), collider.transform.localRotation);
                        rotator.Cloned = true;
                    }
                }
            }
        }
	}
}
