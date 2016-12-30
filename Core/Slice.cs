/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;

namespace Holofunk.Core
{
    /// <summary>
    /// A reference to a sub-segment of an underlying buffer, indexed by the given TTime type.
    /// </summary>
    public struct Slice<TTime, TValue>
        where TValue : struct
    {
        /// <summary>
        /// The backing store; logically divided into slivers.
        /// </summary>
        readonly Buf<TValue> _buffer;

        /// <summary>
        /// The number of slivers contained.
        /// </summary>
        public readonly Duration<TTime> Duration;

        /// <summary>
        /// The index to the sliver at which this slice actually begins.
        /// </summary>
        readonly Duration<TTime> _offset;

        /// <summary>
        /// The size of each sliver in this slice; a count of T.
        /// </summary>
        /// <remarks>
        /// Slices are composed of multiple Slivers, one per unit of Duration.
        /// </remarks>
        public readonly int SliverSize;

        public Slice(Buf<TValue> buffer, Duration<TTime> offset, Duration<TTime> duration, int sliverSize)
        {
            HoloDebug.Assert(buffer.Data != null);
            HoloDebug.Assert(offset >= 0);
            HoloDebug.Assert(duration >= 0);
            HoloDebug.Assert((offset * sliverSize) + (duration * sliverSize) <= buffer.Data.Length);

            _buffer = buffer;
            _offset = offset;
            Duration = duration;
            SliverSize = sliverSize;
        }

        public Slice(Buf<TValue> buffer, int sliverSize)
            : this(buffer, 0, (buffer.Data.Length / sliverSize), sliverSize)
        {
        }

        static Buf<TValue> s_emptyBuf = new Buf<TValue>(0, new TValue[0]);

        public static Slice<TTime, TValue> Empty
        {
            get
            {
                return new Slice<TTime, TValue>(s_emptyBuf, 0, 0, 0);
            }
        }

        public bool IsEmpty() { return Duration == 0; }

        /// <summary>
        /// For use by extension methods only
        /// </summary>
        internal Buf<TValue> Buffer { get { return _buffer; } }

        /// <summary>
        /// For use by extension methods only
        /// </summary>
        internal Duration<TTime> Offset { get { return _offset; } }

        public TValue this[Duration<TTime> offset, int subindex]
        {
            get
            {
                Duration<TTime> totalOffset = _offset + offset;
                Debug.Assert(totalOffset * SliverSize < Buffer.Data.Length);
                long finalOffset = totalOffset * SliverSize + subindex;
                return _buffer.Data[finalOffset];
            }
            set
            {
                Duration<TTime> totalOffset = _offset + offset;
                Debug.Assert(totalOffset < (Buffer.Data.Length / SliverSize));
                long finalOffset = totalOffset * SliverSize + subindex;
                _buffer.Data[finalOffset] = value;
            }
        }

        /// <summary>
        /// Get a portion of this Slice, starting at the given offset, for the given duration.
        /// </summary>
        public Slice<TTime, TValue> Subslice(Duration<TTime> initialOffset, Duration<TTime> duration)
        {
            Debug.Assert(initialOffset >= 0); // can't slice before the beginning of this slice
            Debug.Assert(duration >= 0); // must be nonnegative count
            Debug.Assert(initialOffset + duration <= Duration); // can't slice beyond the end
            return new Slice<TTime, TValue>(_buffer, _offset + initialOffset, duration, SliverSize);
        }

        /// <summary>
        /// Get the rest of this Slice starting at the given offset.
        /// </summary>
        public Slice<TTime, TValue> SubsliceStartingAt(Duration<TTime> initialOffset)
        {
            return Subslice(initialOffset, Duration - initialOffset);
        }

        /// <summary>
        /// Get the prefix of this Slice starting at offset 0 and extending for the requested duration.
        /// </summary>
        public Slice<TTime, TValue> SubsliceOfDuration(Duration<TTime> duration)
        {
            return Subslice(0, duration);
        }

        /// <summary>
        /// Copy this slice's data into destination; destination must be long enough.
        /// </summary>
        public void CopyTo(Slice<TTime, TValue> destination)
        {
            Debug.Assert(destination.Duration >= Duration);
            Debug.Assert(destination.SliverSize == SliverSize);

            // TODO: support backwards copies etc.
            // TODO: what about these int casts?  Is Unity 32-bit only or something?  Why does Array.Copy not have the overload that takes longs?
            // TODO: ensure individual backing arrays never need to go beyond 30-bit indices
            Array.Copy(
                _buffer.Data,
                (int)_offset * SliverSize,
                destination._buffer.Data,
                (int)destination._offset * destination.SliverSize,
                (int)Duration * SliverSize);
        }

        public void CopyFrom(TValue[] source, int sourceOffset, int destinationSubIndex, int subWidth)
        {
            Debug.Assert((int)(sourceOffset + subWidth) <= source.Length);
            int destinationOffset = (int)(_offset * SliverSize + destinationSubIndex);
            Debug.Assert(destinationOffset + subWidth <= _buffer.Data.Length);

            Array.Copy(
                source,
                (int)sourceOffset,
                _buffer.Data,
                destinationOffset,
                subWidth);
        }

        /// <summary>Are these samples adjacent in their underlying storage?</summary>
        public bool Precedes(Slice<TTime, TValue> next)
        {
            return _buffer.Data == next._buffer.Data && _offset + Duration == next._offset;
        }

        /// <summary>Merge two adjacent samples into a single sample.</summary>
        public Slice<TTime, TValue> UnionWith(Slice<TTime, TValue> next)
        {
            HoloDebug.Assert(Precedes(next));
            return new Slice<TTime, TValue>(_buffer, _offset, Duration + next.Duration, SliverSize);
        }

        public override string ToString()
        {
            return "Slice[buffer " + _buffer + ", offset " + _offset + ", duration " + Duration + ", sliverSize " + SliverSize + "]";
        }

        /// <summary>
        /// Equality comparison; deliberately does not implement Equals(object) as this would cause slice boxing.
        /// </summary>
        public bool Equals(Slice<TTime, TValue> other)
        {
            return Buffer.Equals(other.Buffer) && Offset == other.Offset && Duration == other.Duration;
        }
    }
}
