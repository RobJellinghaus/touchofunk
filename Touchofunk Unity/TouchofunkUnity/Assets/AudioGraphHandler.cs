using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioGraphCreator
{
    IAudioGraph CreateAudioGraph();
}

public interface IAudioGraph
{

}

public class AudioGraphHandler : MonoBehaviour {

    /// <summary>
    /// Evil mutable static for dependency injection from the top-level application.
    /// </summary>
    public static IAudioGraphCreator Creator;

    private IAudioGraph _audioGraph;

	// Use this for initialization
	void Start ()
    {
        bool isDebugBuild = Debug.isDebugBuild;
        if (Creator == null) { Debug.Break(); } // App should have injected creator instance
        _audioGraph = Creator.CreateAudioGraph();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
