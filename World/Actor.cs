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
    /// Base class for entities participating in a Holofunk performance.
    /// </summary>
    /// <remarks>
    /// Actors have multiple streams of data associated with them, which may or may not be recorded
    /// locally (as opposed to streamed over the network).
    /// 
    /// Both prerecorded loops and individual performers are varieties of Actor.
    /// </remarks>
    public class Actor
    {
        readonly World _world;

        /// <summary>
        /// Audio data.
        /// </summary>
        readonly DenseSampleFloatStream _audioStream;

        readonly FloatAverager _leftLevelAverager;
        readonly FloatAverager _rightLevelAverager;

        public Actor(World world)
        {

        }
    }
}
