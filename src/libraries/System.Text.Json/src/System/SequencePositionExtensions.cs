// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers;

namespace System
{
    /// <summary>
    /// Extensions methods for <see cref="SequencePosition"/>
    /// </summary>
    internal static class SequencePositionExtensions
    {
        public static SequencePosition PositionOfSegment<T>(this SequencePosition sequencePosition, ReadOnlyMemory<T> segment)
        {
            SequencePosition currentPosition,
                returnValue;

            currentPosition = sequencePosition;

            while (currentPosition.GetObject() is ReadOnlySequenceSegment<byte> currentSegment
                && !segment.Equals(currentSegment.Memory))
            {
                currentPosition = new SequencePosition(currentSegment.Next, 0);
            }

            returnValue = currentPosition;
            return returnValue;
        }
    }
}
