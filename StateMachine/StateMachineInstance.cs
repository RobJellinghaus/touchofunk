/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using Holofunk.Core;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Holofunk.StateMachines
{
    public static class ListExtensions
    {
        public static void AddFrom<T>(this List<T> thiz, List<T> other, int start, int count)
        {
            HoloDebug.Assert(thiz != null && other != null);
            HoloDebug.Assert(start >= 0 && count >= 0);
            HoloDebug.Assert(start + count <= other.Count);

            for (int i = start; i < start + count; i++) {
                thiz.Add(other[i]);
            }
        }
    }

    /// <summary>A running instantiation of a particular StateMachine.</summary>
    public class StateMachineInstance<TEvent> 
    {
        readonly StateMachine<TEvent> _machine;
        State<TEvent> _machineState;

        // The model is dependent on where in the hierarchy we are; it may be transformed on
        // entry or exit.
        Model _model;

        // Reused stacks for finding common parents.
        readonly List<State<TEvent>> _startList = new List<State<TEvent>>();
        readonly List<State<TEvent>> _endList = new List<State<TEvent>>();
        readonly List<State<TEvent>> _pathDownList = new List<State<TEvent>>();

        Moment _lastTransitionMoment;

        public StateMachineInstance(TEvent initial, StateMachine<TEvent> machine, Model initialModel)
        {
            _machine = machine;
            _machineState = machine.RootState;
            _model = initialModel;

            MoveTo(initial, machine.InitialState);
        }

        public Moment LastTransitionMoment { get { return _lastTransitionMoment; } set { _lastTransitionMoment = value; } }

        // We are in state start.  We need to get to state end.
        // Do so by performing all the exit actions necessary to get up to the common parent of start and end,
        // and then all the enter actions necessary to get down to end from that common parent.
        void MoveTo(TEvent evt, State<TEvent> end)
        {
            // if already there, do nothing
            if (_machineState == end) {
                return;
            }

            // Get the common parent of start and end.
            // This will be null if they have no common parent.
            State<TEvent> commonParent = GetCommonParent(_machineState, end, _pathDownList);

            ExitUpTo(evt, _machineState, commonParent);
            EnterDownTo(evt, _pathDownList);

            _machineState = end;
        }


        void ExitUpTo(TEvent evt, State<TEvent> state, State<TEvent> commonParent)
        {
            while (state != commonParent) {
                _model = state.Exit(evt, _model);
                state = state.Parent;
            }
        }

        void EnterDownTo(TEvent evt, List<State<TEvent>> pathToEnd)
        {
            for (int i = 0; i < pathToEnd.Count; i++) {
                _model = pathToEnd[i].Enter(evt, _model);
            }
        }

        State<TEvent> GetCommonParent(
            State<TEvent> start,
            State<TEvent> end,
            List<State<TEvent>> pathDownToEnd)
        {
            // we don't handle this case!
            HoloDebug.Assert(start != end);

            if (start == null || end == null) {
                return null;
            }

            // make a list of all states to root.
            // (actually, the lists wind up being ordered from root to the leaf state.)
            ListToRoot(start, _startList);
            ListToRoot(end, _endList);

            // now the common parent is the end of the longest common prefix.
            pathDownToEnd.Clear();
            for (int i = 0; i < Math.Min(_startList.Count, _endList.Count); i++) {
                if (_startList[i] != _endList[i]) {
                    if (i == 0) {
                        pathDownToEnd.AddFrom(_endList, 0, _endList.Count);
                        return null;
                    }
                    else {
                        pathDownToEnd.AddFrom(_endList, i - 1, _endList.Count - i + 1);
                        return _startList[i - 1];
                    }
                }
            }

            // If we got to here, then one list is a prefix of the other.

            if (_startList.Count > _endList.Count) {
                // The start list is longer, so end contains (hierarchically speaking) start.
                // So there IS no pathDownToEnd, and the end of endList is the common parent.
                return _endList[_endList.Count - 1];
            }
            else {
                // _endList is longer.
                pathDownToEnd.AddFrom(_endList, _startList.Count, _endList.Count - _startList.Count);
                return _startList[_startList.Count - 1];
            }
        }

        // Clear list and replace it with the ancestor chain of state, with the root at index 0.
        void ListToRoot(State<TEvent> state, List<State<TEvent>> list)
        {
            list.Clear();

            while (state != null) {
                list.Add(state);
                state = state.Parent;
            }

            list.Reverse();
        }

        public void OnCompleted()
        {
            // we don't do nothin' (yet)
        }

        public void OnError(Exception exception)
        {
            Debug.WriteLine(exception.ToString());
            Debug.WriteLine(exception.StackTrace);
            HoloDebug.Assert(false);
        }

        public void OnNext(TEvent value, Moment now)
        {
            // Find transition if any.
            var destination = _machine.TransitionFrom(_machineState, value, _model);
            if (destination != null) {
                MoveTo(value, destination);
            }

            _lastTransitionMoment = now;
        }

        public void GameUpdate(Moment now)
        {
            _model.GameUpdate(now);
        }
    }
}
