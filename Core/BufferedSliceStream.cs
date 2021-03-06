/////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2011-2017 by Rob Jellinghaus.                             //
// Licensed under MIT license, http://github.com/RobJellinghaus/Touchofunk //
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Holofunk.Core
{
    /// <summary>
    /// A stream that buffers some amount of data in memory.
    /// </summary>
    public abstract class BufferedSliceStream<TTime, TValue> : DenseSliceStream<TTime, TValue>
        where TValue : struct
    {
        /// <summary>
        /// Allocator for buffer management.
        /// </summary>
        readonly BufferAllocator<TValue> _allocator;

        /// <summary>
        /// The slices making up the buffered data itself.
        /// </summary>
        /// <remarks>
        /// The InitialTime of each entry in this list must exactly equal the InitialTime + Duration of the
        /// previous entry; in other words, these are densely arranged in time.
        /// </remarks>
        readonly List<TimedSlice<TTime, TValue>> _data = new List<TimedSlice<TTime, TValue>>();

        /// <summary>
        /// The maximum amount that this stream will buffer while it is open; more appends will cause
        /// earlier data to be dropped.  If 0, no buffering limit will be enforced.
        /// </summary>
        readonly Duration<TTime> _maxBufferedDuration;

        /// <summary>
        /// Temporary space for, e.g., the IntPtr Append method.
        /// </summary>
        readonly Buf<TValue> _tempBuffer = new Buf<TValue>(-1, new TValue[1024]); // -1 id = temp buf

        /// <summary>
        /// This stream holds onto an entire buffer and copies data into it when appending.
        /// </summary>
        Slice<TTime, TValue> _remainingFreeBuffer;

        /// <summary>
        /// The mapper that converts absolute time into relative time for this stream.
        /// </summary>
        IntervalMapper<TTime> _intervalMapper;

        /// <summary>
        /// Action to copy IntPtr data into a Slice.
        /// </summary>
        /// <remarks>
        /// Since .NET offers no way to marshal into an array of generic type, we can't express this
        /// function cleanly except in a specialized method defined in a subclass.
        /// </remarks>
        readonly Action<IntPtr, Slice<TTime, TValue>> _copyIntPtrToSliceAction;

        /// <summary>
        /// Action to copy Slice data into an IntPtr.
        /// </summary>
        /// <remarks>
        /// Since .NET offers no way to marshal into an array of generic type, we can't express this
        /// function cleanly except in a specialized method defined in a subclass.
        /// </remarks>
        readonly Action<Slice<TTime, TValue>, IntPtr> _copySliceToIntPtrAction;

        /// <summary>
        /// Action to obtain an IntPtr directly on a Slice's data, and invoke another action with that IntPtr.
        /// </summary>
        /// <remarks>
        /// Again, since .NET does not allow taking the address of a generic array, we must use a
        /// specialized implementation wrapped in this generic signature.
        /// </remarks>
        readonly Action<Slice<TTime, TValue>, Action<IntPtr, int>> _rawSliceAccessAction;

        readonly bool _useContinuousLoopingMapper = false;

        public BufferedSliceStream(
            Time<TTime> initialTime,
            BufferAllocator<TValue> allocator,
            int sliverSize,
            Action<IntPtr, Slice<TTime, TValue>> copyIntPtrToSliceAction,
            Action<Slice<TTime, TValue>, IntPtr> copySliceToIntPtrAction,
            Action<Slice<TTime, TValue>, Action<IntPtr, int>> rawSliceAccessAction,
            Duration<TTime> maxBufferedDuration = default(Duration<TTime>),
            bool useContinuousLoopingMapper = false)
            : base(initialTime, sliverSize)
        {
            _allocator = allocator;
            _copyIntPtrToSliceAction = copyIntPtrToSliceAction;
            _copySliceToIntPtrAction = copySliceToIntPtrAction;
            _rawSliceAccessAction = rawSliceAccessAction;
            _maxBufferedDuration = maxBufferedDuration;
            _useContinuousLoopingMapper = useContinuousLoopingMapper;

            // as long as we are appending, we use the identity mapping
            // TODO: support delay mapping
            _intervalMapper = new IdentityIntervalMapper<TTime, TValue>(this);
        }

        public override string ToString()
        {
            return "BufferedSliceStream[" + InitialTime + ", " + DiscreteDuration + "]";
        }

        void EnsureFreeBuffer()
        {
            if (_remainingFreeBuffer.IsEmpty()) {
                Buf<TValue> chunk = _allocator.Allocate();
                _remainingFreeBuffer = new Slice<TTime, TValue>(
                    chunk,
                    0,
                    (chunk.Data.Length / SliverSize),
                    SliverSize);
            }
        }

        public override void Shut(ContinuousDuration finalDuration)
        {
            base.Shut(finalDuration);
            // swap out our mappers, we're looping now
            if (_useContinuousLoopingMapper) {
                _intervalMapper = new LoopingIntervalMapper<TTime, TValue>(this);
            }
            else {
                _intervalMapper = new SimpleLoopingIntervalMapper<TTime, TValue>(this);
            }

#if SPAMAUDIO
            foreach (TimedSlice<TTime, TValue> timedSlice in _data) {
                Spam.Audio.WriteLine("BufferedSliceStream.Shut: next slice time " + timedSlice.InitialTime + ", slice " + timedSlice.Slice);
            }
#endif
        }

        /// <summary>
        /// Return a temporary buffer slice of the given duration or the max temp buffer size, whichever is lower.
        /// </summary>
        /// <param name="duration"></param>
        /// <returns></returns>
        Slice<TTime, TValue> TempSlice(Duration<TTime> duration)
        {
            Duration<TTime> maxDuration = _tempBuffer.Data.Length / SliverSize;
            return new Slice<TTime, TValue>(
                _tempBuffer,
                0,
                duration > maxDuration ? maxDuration : duration,
                SliverSize);
        }

        /// <summary>
        /// Append the given amount of data marshalled from the pointer P.
        /// </summary>
        public override void Append(Duration<TTime> duration, IntPtr p)
        {
            HoloDebug.Assert(!IsShut);

            while (duration > 0) {
                Slice<TTime, TValue> tempSlice = TempSlice(duration);

                _copyIntPtrToSliceAction(p, tempSlice);
                Append(tempSlice);
                duration -= tempSlice.Duration;
            }
            _discreteDuration += duration;
        }

        /// <summary>
        /// Append this slice's data, by copying it into this stream's private buffers.
        /// </summary>
        public override void Append(Slice<TTime, TValue> source)
        {
            HoloDebug.Assert(!IsShut);

            // Try to keep copying source into _remainingFreeBuffer
            while (!source.IsEmpty()) {
                EnsureFreeBuffer();

                // if source is larger than available free buffer, then we'll iterate
                Slice<TTime, TValue> originalSource = source;
                if (source.Duration > _remainingFreeBuffer.Duration) {
                    source = source.Subslice(0, _remainingFreeBuffer.Duration);
                }

                // now we know source can fit
                Slice<TTime, TValue> dest = _remainingFreeBuffer.SubsliceOfDuration(source.Duration);
                source.CopyTo(dest);

                // dest may well be adjacent to the previous slice, if there is one, since we may
                // be appending onto an open chunk.  So here is where we coalesce this, if so.
                dest = InternalAppend(dest);

                // and update our loop variables
                source = originalSource.SubsliceStartingAt(source.Duration);

                Trim();
            }
        }

        /// <summary>
        /// Internally append this slice (which must be allocated from our free buffer); this does the work
        /// of coalescing, updating _data and other fields, etc.
        /// </summary>
        Slice<TTime, TValue> InternalAppend(Slice<TTime, TValue> dest)
        {
            // dest must be from our free buffer
            HoloDebug.Assert(dest.Buffer.Data == _remainingFreeBuffer.Buffer.Data);

            if (_data.Count == 0) {
                _data.Add(new TimedSlice<TTime, TValue>(InitialTime, dest));
            }
            else {
                TimedSlice<TTime, TValue> last = _data[_data.Count - 1];
                if (last.Slice.Precedes(dest)) {
                    _data[_data.Count - 1] = new TimedSlice<TTime, TValue>(last.InitialTime, last.Slice.UnionWith(dest));
                }
                else {
                    Spam.Audio.WriteLine("BufferedSliceStream.InternalAppend: last did not precede; last slice is " + last.Slice + ", last slice time " + last.InitialTime + ", dest is " + dest);
                    _data.Add(new TimedSlice<TTime, TValue>(last.InitialTime + last.Slice.Duration, dest));
                }
            }

            _discreteDuration += dest.Duration;
            _remainingFreeBuffer = _remainingFreeBuffer.SubsliceStartingAt(dest.Duration);
            
            return dest;
        }

        /// <summary>
        /// Copy strided data from a source array into a single destination sliver.
        /// </summary>
        public override void AppendSliver(TValue[] source, int startOffset, int width, int stride, int height)
        {
            HoloDebug.Assert(source != null);
            int neededLength = startOffset + stride * (height - 1) + width;
            HoloDebug.Assert(source.Length >= neededLength);
            HoloDebug.Assert(SliverSize == width * height);
            HoloDebug.Assert(stride >= width);

            EnsureFreeBuffer();

            Slice<TTime, TValue> destination = _remainingFreeBuffer.SubsliceOfDuration(1);

            int sourceOffset = startOffset;
            int destinationOffset = 0;
            for (int h = 0; h < height; h++) {
                destination.CopyFrom(source, sourceOffset, destinationOffset, width);

                sourceOffset += stride;
                destinationOffset += width;
            }

            InternalAppend(destination);

            Trim();
        }

        /// <summary>
        /// Trim off any content beyond the maximum allowed to be buffered.
        /// </summary>
        /// <remarks>
        /// Internal because wrapper streams want to delegate to this when they are themselves Trimmed.</remarks>
        void Trim()
        {
            if (_maxBufferedDuration == 0 || _discreteDuration <= _maxBufferedDuration) {
                return;
            }

            while (DiscreteDuration > _maxBufferedDuration) {
                Duration<TTime> toTrim = DiscreteDuration - _maxBufferedDuration;
                // get the first slice
                TimedSlice<TTime, TValue> firstSlice = _data[0];
                if (firstSlice.Slice.Duration <= toTrim) {
                    _data.RemoveAt(0);
#if DEBUG
                    // check to make sure our later stream data doesn't reference this one we're about to free
                    foreach (TimedSlice<TTime, TValue> slice in _data) {
                        HoloDebug.Assert(slice.Slice.Buffer.Data != firstSlice.Slice.Buffer.Data);
                    }
#endif
                    _allocator.Free(firstSlice.Slice.Buffer);
                    _discreteDuration -= firstSlice.Slice.Duration;
                    _initialTime += firstSlice.Slice.Duration;
                }
                else {
                    TimedSlice<TTime, TValue> newFirstSlice = new TimedSlice<TTime, TValue>(
                        firstSlice.InitialTime + toTrim,
                        new Slice<TTime, TValue>(
                                firstSlice.Slice.Buffer,
                                firstSlice.Slice.Offset + toTrim,
                                firstSlice.Slice.Duration - toTrim,
                                SliverSize));
                    _data[0] = newFirstSlice;
                    _discreteDuration -= toTrim;
                    _initialTime += toTrim;
                }
            }
        }

        public override void CopyTo(Interval<TTime> sourceInterval, IntPtr p)
        {
            while (!sourceInterval.IsEmpty) {
                Slice<TTime, TValue> source = GetNextSliceAt(sourceInterval);
                _copySliceToIntPtrAction(source, p);
                sourceInterval = sourceInterval.SubintervalStartingAt(source.Duration);
            }
        }

        public override void CopyTo(Interval<TTime> sourceInterval, DenseSliceStream<TTime, TValue> destinationStream)
        {
            while (!sourceInterval.IsEmpty) {
                Slice<TTime, TValue> source = GetNextSliceAt(sourceInterval);
                destinationStream.Append(source);
                sourceInterval = sourceInterval.SubintervalStartingAt(source.Duration);
            }
        }

        /// <summary>
        /// Map the interval time to stream local time, and get the next slice of it.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public override Slice<TTime, TValue> GetNextSliceAt(Interval<TTime> interval)
        {
            Interval<TTime> firstMappedInterval = _intervalMapper.MapNextSubInterval(interval);

            if (firstMappedInterval.IsEmpty) {
                return Slice<TTime, TValue>.Empty;
            }

            HoloDebug.Assert(firstMappedInterval.InitialTime >= InitialTime);
            HoloDebug.Assert(firstMappedInterval.InitialTime + firstMappedInterval.Duration <= InitialTime + DiscreteDuration);

            TimedSlice<TTime, TValue> foundTimedSlice = GetInitialTimedSlice(firstMappedInterval);
            Interval<TTime> intersection = foundTimedSlice.Interval.Intersect(firstMappedInterval);
            HoloDebug.Assert(!intersection.IsEmpty);
            Slice<TTime, TValue> ret = foundTimedSlice.Slice.Subslice(
                intersection.InitialTime - foundTimedSlice.InitialTime,
                intersection.Duration);

            return ret;
        }

        TimedSlice<TTime, TValue> GetInitialTimedSlice(Interval<TTime> firstMappedInterval)
        {
            // we must overlap somewhere
            HoloDebug.Assert(!firstMappedInterval.Intersect(new Interval<TTime>(InitialTime, DiscreteDuration)).IsEmpty);

            // Get the biggest available slice at firstMappedInterval.InitialTime.
            // First, get the index of the slice just after the one we want.
            TimedSlice<TTime, TValue> target = new TimedSlice<TTime, TValue>(firstMappedInterval.InitialTime, Slice<TTime, TValue>.Empty);
            int originalIndex = _data.BinarySearch(target, TimedSlice<TTime, TValue>.Comparer.Instance);
            int index = originalIndex;

            if (index < 0) {
                // index is then the index of the next larger element
                // -- we know there is a smaller element because we know firstMappedInterval fits inside stream interval
                index = (~index) - 1;
                HoloDebug.Assert(index >= 0);
            }

            TimedSlice<TTime, TValue> foundTimedSlice = _data[index];
            return foundTimedSlice;
        }

        public override void Dispose()
        {
            // release each T[] back to the buffer
            foreach (TimedSlice<TTime, TValue> slice in _data) {
                // this requires that Free be idempotent; in general we don't expect
                // many slices per buffer, since each Stream allocates from a private
                // buffer and coalesces aggressively
                _allocator.Free(slice.Slice.Buffer);
            }
        }
    }

    public static class SliceFloatExtension
    {
        /// <summary>
        /// Copy data from IntPtr to Slice.
        /// </summary>
        public static void CopyToSlice<TTime>(this IntPtr src, Slice<TTime, float> dest)
        {
            Marshal.Copy(src, dest.Buffer.Data, (int)dest.Offset * dest.SliverSize, (int)dest.Duration * dest.SliverSize);
        }
        /// <summary>
        /// Copy data from Slice to IntPtr.
        /// </summary>
        public static void CopyToIntPtr<TTime>(this Slice<TTime, float> src, IntPtr dest)
        {
            Marshal.Copy(src.Buffer.Data, (int)src.Offset * src.SliverSize, dest, (int)src.Duration * src.SliverSize);
        }

        /// <summary>
        /// Invoke some underlying action with an IntPtr directly to a Slice's data.
        /// </summary>
        /// <remarks>
        /// The arguments to the action are an IntPtr to the fixed data, and the number of BYTES to act on.
        /// </remarks>
        public static unsafe void RawAccess<TTime>(this Slice<TTime, float> src, Action<IntPtr, int> action)
        {
            // per http://www.un4seen.com/forum/?topic=12912.msg89978#msg89978
            fixed (float* p = &src.Buffer.Data[src.Offset * src.SliverSize]) {
                byte* b = (byte*)p;

                action(new IntPtr(p), (int)src.Duration * src.SliverSize * sizeof(float));
            }
        }
    }

    public static class SliceByteExtension
    {
        /// <summary>
        /// Copy data from IntPtr to Slice.
        /// </summary>
        public static void CopyToSlice<TTime>(this IntPtr src, Slice<TTime, byte> dest)
        {
            Marshal.Copy(src, dest.Buffer.Data, (int)dest.Offset * dest.SliverSize, (int)dest.Duration * dest.SliverSize);
        }
        /// <summary>
        /// Copy data from Slice to IntPtr.
        /// </summary>
        public static void CopyToIntPtr<TTime>(this Slice<TTime, byte> src, IntPtr dest)
        {
            Marshal.Copy(src.Buffer.Data, (int)src.Offset * src.SliverSize, dest, (int)src.Duration * src.SliverSize);
        }
        /// <summary>
        /// Invoke some underlying action with an IntPtr directly to a Slice's data.
        /// </summary>
        /// <remarks>
        /// The action receives an IntPtr to the data, and an int that is a count of bytes.
        /// </remarks>
        public static unsafe void RawAccess<TTime>(this Slice<TTime, byte> src, Action<IntPtr, int> action)
        {
            // per http://www.un4seen.com/forum/?topic=12912.msg89978#msg89978
            fixed (byte* p = &src.Buffer.Data[src.Offset * src.SliverSize]) {

                action(new IntPtr(p), (int)src.Duration * src.SliverSize);
            }
        }
    }

    /// <summary>
    /// A dense stream of float samples indexed by discrete sample time.
    /// </summary>
    public class DenseSampleFloatStream : BufferedSliceStream<Sample, float>
    {
        public DenseSampleFloatStream(
            Time<Sample> initialTime,
            BufferAllocator<float> allocator,
            int sliverSize,
            Duration<Sample> maxBufferedDuration = default(Duration<Sample>),
            bool useContinuousLoopingMapper = false)
            : base(initialTime,
                allocator,
                sliverSize,
                SliceFloatExtension.CopyToSlice,
                SliceFloatExtension.CopyToIntPtr,
                SliceFloatExtension.RawAccess,
                maxBufferedDuration,
                useContinuousLoopingMapper)
        {
        }

        public override int SizeofValue()
        {
            return sizeof(float);
        }
    }

    public class DenseFrameByteStream : BufferedSliceStream<Frame, byte>
    {
        public DenseFrameByteStream(
            BufferAllocator<byte> allocator,
            int sliverSize,
            Duration<Frame> maxBufferedDuration = default(Duration<Frame>),
            bool useContinuousLoopingMapper = false)
            : base(0,
                allocator,
                sliverSize,
                SliceByteExtension.CopyToSlice,
                SliceByteExtension.CopyToIntPtr,
                SliceByteExtension.RawAccess,
                maxBufferedDuration,
                useContinuousLoopingMapper)
        {
        }

        public override int SizeofValue()
        {
            return sizeof(byte);
        }
    }

    public class SparseSampleByteStream : SparseSliceStream<Sample, byte>
    {
        public SparseSampleByteStream(
            Time<Sample> initialTime,
            BufferAllocator<byte> allocator,
            int sliverSize,
            int maxBufferedFrameCount = 0)
            : base(initialTime, 
                new DenseFrameByteStream(allocator, sliverSize, maxBufferedFrameCount), 
                maxBufferedFrameCount)
        {
        }

        public override int SizeofValue()
        {
            return sizeof(byte);
        }
    }
}
