/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace Holofunk.Core
{
    class HoloDebugException : Exception
    {
    }

    public class HoloDebug
    {
        /// <summary>Assertion dialogs can hose Holofunk; this trivial wrapper lets us breakpoint just before we dialog.</summary>
        /// <param name="value"></param>
        public static void Assert(bool value, string message = null)
        {
            if (!value) {
                throw new HoloDebugException();
                Debug.Assert(value, message);
            }
        }
    }
}
