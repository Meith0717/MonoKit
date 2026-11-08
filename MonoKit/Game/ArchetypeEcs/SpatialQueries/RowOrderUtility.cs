// AppHost - RowOrderUtility.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;

namespace MonoKit.Game.ArchetypeEcs.SpatialQueries;

internal static class RowOrderUtility
{
    /// <summary>Reorders a column in place to match the given sorted row order.</summary>
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
        [ThreadStatic]
        private static T[]? _buffer;

        /// <summary>Returns the thread-static buffer, growing it as needed.</summary>
        public static T[] Get(int minLength)
        {
            var buffer = _buffer;
            if (buffer == null || buffer.Length < minLength)
                _buffer = buffer = new T[int.Max(minLength, (buffer?.Length ?? 0) * 2)];
            return buffer;
        }
    }
}
