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
    /// Container of Actors; holds shared infrastructure for creating new Loopies etc.
    /// </summary>
    public class World
    {
        Clock _clock;
        internal BufferAllocator<float> _audioAllocator;

        public World(BufferAllocator<float> audioAllocator)
        {
            _audioAllocator = audioAllocator;

        }

        public Clock Clock { get { return _clock; } }
    }
}
