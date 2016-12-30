/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

namespace Holofunk.Core
{
    public struct Option<T>
    {
        readonly bool _hasValue;
        readonly T _value;

        public static Option<T> None
        {
            get { return default(Option<T>); }
        }

        public Option(T value)
        {
            _hasValue = true;
            _value = value;
        }

        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }

        public T Value
        {
            get { HoloDebug.Assert(_hasValue); return _value; }
        }

        public bool HasValue
        {
            get { return _hasValue; }
        }
    }
}
