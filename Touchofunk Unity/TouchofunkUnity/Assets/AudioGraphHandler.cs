/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHoloAudioGraph
{

}

public class AudioGraphHandler : MonoBehaviour {

    /// <summary>
    /// Evil mutable static for dependency injection from the top-level application.
    /// </summary>
    public static IHoloAudioGraph AudioGraph;

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
