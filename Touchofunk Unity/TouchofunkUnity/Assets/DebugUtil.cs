using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtil : MonoBehaviour
{
    public static void Assert(bool value, string assertion = null)
    {
        if (!value)
        {
            // Do whatever can be done in unity even if it doesn't affect VS at all.
            // Breakpoint here when running in VS under the debugger.
            Debug.Assert(value, assertion);
        }
    }

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {		
	}
}
