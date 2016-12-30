/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Holofunk.StateMachines
{
    public class Transition<TEvent>
    {
        readonly TEvent _event;

        public Transition(TEvent evt)
        {
            _event = evt;
        }

        public TEvent Event { get { return _event; } }

    }

    /// <summary>A transition in a StateMachine.</summary>
    /// <remarks>Is labeled with an event, and contains a means to compute a destination state.</remarks>
    public class Transition<TEvent, TModel> : Transition<TEvent>
    {
        readonly Func<TEvent, TModel, State<TEvent>> _destinationFunc;

        public Transition(
            TEvent evt,
            Func<TEvent, TModel, State<TEvent>> destinationFunc)
            : base(evt)
        {
            _destinationFunc = destinationFunc;
        }

        public Transition(
            TEvent evt,
            State<TEvent> destinationState)
            : this(evt, (ignoreModel, ignoreEvent) => destinationState)
        {
        }

        /// <summary>
        /// compute the destination state
        /// </summary>
        public State<TEvent> ComputeDestination(TEvent evt, TModel model)
        {
            return _destinationFunc(evt, model);
        }
    }
}
