/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using Holofunk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Holofunk.World
{
    /// <summary>
    /// The core constant data values needed by the World.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Latency compensation, in sample duration.
        /// </summary>
        /// <remarks>
        /// This is very much input-modality-dependent so eventually we will make this part of some UI
        /// reification, but not in initial Touchofunk.
        /// </remarks>
        public readonly static Duration<Sample> LatencyCompensationDuration = 100;

        /// <summary>
        /// Quarter second duration over which volume is averaged.
        /// </summary>
        public readonly static Duration<Sample> VolumeAveragerDuration = SampleRateHz / 4;

        /// <summary>
        /// We hardcode to 48Khz sample rate as this seems to be AudioGraph default.
        /// </summary>
        public const int SampleRateHz = 48000;

        // 4/4 time (actually, 4/_ time, we don't care about the note duration)
        public const int BeatsPerMeasure = 4;

        // what tempo do we start at?
        // turnado sync: 130.612f (120bpm x 48000/44100)
        public readonly static float InitialBpm = 90;


    }
}
