/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

namespace Holofunk.Core
{
    /// <summary>
    /// A continous distance between two Times.
    /// </summary>                                                                                           
    public struct ContinuousDuration
    {
        /// <summary>
        /// Duration in seconds.
        /// </summary>
        readonly double _duration;

        public ContinuousDuration(double duration)
        {
            _duration = duration;
        }

        public static explicit operator double(ContinuousDuration duration)
        {
            return duration._duration;
        }

        public static explicit operator ContinuousDuration(double value)
        {
            return new ContinuousDuration(value);
        }
    }
}
