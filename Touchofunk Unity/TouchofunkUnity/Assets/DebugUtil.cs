using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class DebugUtil : MonoBehaviour
{
    public static void Assert(bool value, string assertion = null)
    {
        if (!value)
        {
            // Do whatever can be done in unity even if it doesn't affect VS at all.
            // Breakpoint here when running in VS under the debugger.
            UnityEngine.Debug.Assert(value, assertion);
        }
    }

    public static void Requires(bool value)
    {
        Assert(value, "Contractual requirement");
    }

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update ()
    {		
	}

    struct ThreadState
    {
        public readonly bool Initialized;
        public readonly int ManagedThreadId;
        public readonly string Name;
        public readonly string StringValue;

        public ThreadState(Thread thread, string name)
        {
            DebugUtil.Requires(thread != null);

            Initialized = true;
            ManagedThreadId = thread.ManagedThreadId;
            Name = name;
            StringValue = thread.ToString();
        }
    }

    static object s_threadLock = new object();

    /// <summary>
    /// Track main thread such that we can verify we are on it.
    /// </summary>
    /// <remarks>Shows as "Main Thread" in debugger under VS2015 Windows 10 14939</remarks>
    static ThreadState s_mainThreadState = default(ThreadState);

    /// <summary>
    /// Track app thread such that we can verify we are on it.
    /// </summary>
    /// <remarks>Shows as "SHCore.dll thread" in debugger under VS2015 Windows 10 14939</remarks>
    static ThreadState s_appThreadState = default(ThreadState);

    /// <summary>
    /// Track Unity thread such that we can verify we are on it.
    /// </summary>
    static ThreadState s_unityThreadState = default(ThreadState);

    /// <summary>
    /// Track AudioGraph thread so we can verify we are on it.
    /// </summary>
    static ThreadState s_audioGraphThreadState = default(ThreadState);

    [Conditional("DEBUG")]
    public static void CheckMainThread()
    {
        CheckThread(ref s_mainThreadState, "Main");
    }

    [Conditional("DEBUG")]
    public static void CheckAppThread()
    {
        CheckThread(ref s_appThreadState, "App");
    }

    [Conditional("DEBUG")]
    public static void CheckUnityThread()
    {
        CheckThread(ref s_unityThreadState, "Unity");
    }

    [Conditional("DEBUG")]
    public static void CheckAudioGraphThread()
    {
        CheckThread(ref s_audioGraphThreadState, "AudioGraph");
    }

    static void CheckThread(ref ThreadState threadStateRef, string name)
    {
        if (!threadStateRef.Initialized)
        {
            lock (s_threadLock)
            {
                if (!threadStateRef.Initialized)
                {
                    threadStateRef = new ThreadState(Thread.CurrentThread, name);
                }
            }
        }

        if (Thread.CurrentThread.ManagedThreadId != threadStateRef.ManagedThreadId)
        {
            ThreadState currentThreadState = new ThreadState(Thread.CurrentThread, "Unexpected");
            Assert(false, string.Format("Current thread expected to be {0} with managed id {1}, but got managed id {2} (string value {3})",
                name, threadStateRef.ManagedThreadId, currentThreadState.ManagedThreadId, currentThreadState.StringValue));
        }
    }
}
