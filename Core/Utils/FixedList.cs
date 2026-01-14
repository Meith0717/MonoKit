// FixedList.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using System.Collections.Generic;

namespace MonoKit.Core.Utils;

public class FixedList<T>(int capacity)
{
    private readonly T[] _items = new T[capacity];

    public int Count { get; private set; }

    public int Capacity => _items.Length;

    public bool Full => Capacity == Count;

    public T this[int index]
    {
        get
        {
            if (index >= Count || index < 0)
                throw new ArgumentOutOfRangeException();

            return _items[index];
        }
    }

    public bool TryAdd(T item)
    {
        if (Count >= _items.Length)
            return false;

        _items[Count++] = item;
        return true;
    }

    public void RemoveAt(int index)
    {
        if (index >= Count || index < 0)
            throw new ArgumentOutOfRangeException();

        for (var i = index; i < Count - 1; i++)
            _items[i] = _items[i + 1];

        _items[--Count] = default!;
    }

    public bool TryRemove(T item)
    {
        var index = IndexOf(item);
        if (index == -1)
            return false;

        RemoveAt(index);
        return true;
    }

    public int IndexOf(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < Count; i++)
            if (comparer.Equals(_items[i], item))
                return i;
        return -1;
    }

    public void Clear()
    {
        Array.Clear(_items, 0, Count);
        Count = 0;
    }

    public T[] ToArray()
    {
        return _items;
    }
}
