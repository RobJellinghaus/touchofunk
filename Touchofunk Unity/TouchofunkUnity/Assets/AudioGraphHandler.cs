using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioGraph
{

}

public class AudioGraphHandler : MonoBehaviour {

    /// <summary>
    /// Evil mutable static for dependency injection from the top-level application.
    /// </summary>
    public static IAudioGraph AudioGraph;

	// Use this for initialization
	void Start ()
    {
        // TODO: create debug idiom
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}
