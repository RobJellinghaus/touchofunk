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
    public class Player : Actor
    {
        int _audioInputChannel;

        public Player(World world, int audioInputChannel)
            : base(
                  world,
                  new DenseSampleFloatStream(
                    default(Time<Sample>),
                    world.AudioAllocator,
                    1, // input channels are mono
                    maxBufferedDuration: Clock.TimepointRateHz)) // buffer only up to one second)
        {
            DebugUtil.Requires(world != null);
            DebugUtil.Requires(audioInputChannel >= 0);

            _audioInputChannel = audioInputChannel;

            // TODO: wire up audio graph
        }
    }
}
