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
        BufferAllocator<float> _audioAllocator;
        AudioGraphImpl _audioGraph;

        List<Player> _players = new List<Player>();
        List<Loopie> _loopies = new List<Loopie>();

        public World(AudioGraphImpl audioGraphImpl)
        {
            // Each buffer is one second of audio at two stereo channels x 4 bytes per 32-bit-float sample x sample rate.
            // Allocate 128 of them, arbitrarily.
            _audioAllocator = new BufferAllocator<float>(2 * 4 * Constants.SampleRateHz, 128, sizeof(float));
            _audioGraph = audioGraphImpl;
        }

        public Clock Clock { get { return _clock; } }

        internal BufferAllocator<float> AudioAllocator { get { return _audioAllocator; } }

        internal AudioGraphImpl AudioGraph { get { return _audioGraph; } }

        public Player CreatePlayer(int initialAudioChannel)
        {
            Player newPlayer = new Player(this, initialAudioChannel);
            _players.Add(newPlayer);
            return newPlayer;
        }
    }
}
