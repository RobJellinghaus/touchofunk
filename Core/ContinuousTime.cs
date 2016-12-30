/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

namespace Holofunk.Core
{
    /// <summary>
    /// A continous time measured in floating point seconds.
    /// </summary>                                                                                           
    public struct ContinuousTime
    {
        /// <summary>
        /// Duration in seconds.
        /// </summary>
        readonly double _timeInSeconds;

        public ContinuousTime(double timeInSeconds)
        {
            _timeInSeconds = timeInSeconds;
        }

        public static explicit operator double(ContinuousTime time)
        {
            return time._timeInSeconds;
        }

        public static explicit operator ContinuousTime(double value)
        {
            return new ContinuousTime(value);
        }
    }
}
