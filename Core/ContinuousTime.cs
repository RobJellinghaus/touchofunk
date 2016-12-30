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
        readonly double m_timeInSeconds;

        public ContinuousTime(double timeInSeconds)
        {
            m_timeInSeconds = timeInSeconds;
        }

        public static explicit operator double(ContinuousTime time)
        {
            return time.m_timeInSeconds;
        }

        public static explicit operator ContinuousTime(double value)
        {
            return new ContinuousTime(value);
        }
    }
}
