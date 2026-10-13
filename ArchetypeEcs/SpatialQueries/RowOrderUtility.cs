// RowOrderUtility.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace MonoKit.ArchetypeEcs.SpatialQueries;

internal static class RowOrderUtility
{
    internal static void PermuteColumn<T>(T[] column, int[] order, int rowCount)
        where T : struct
    {
        var buffer = OrderBuffer<T>.Get(rowCount);
        for (var i = 0; i < rowCount; i++)
            buffer[i] = column[order[i]];
        Array.Copy(buffer, column, rowCount);
    }

    private static class OrderBuffer<T>
        where T : struct
    {
        private static T[] _buffer = [];

        public static T[] Get(int minLength)
        {
            if (_buffer.Length < minLength)
                _buffer = new T[int.Max(minLength, _buffer.Length * 2)];
            return _buffer;
        }
    }
}
