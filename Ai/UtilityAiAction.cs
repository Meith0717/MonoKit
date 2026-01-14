// AIAction.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.Ai;

public abstract class UtilityAiAction : IAiAction
{
    private readonly List<IConsideration> _considerations = [];

    public abstract string Name { get; }

    public float Evaluate()
    {
        if (_considerations.Count == 0)
            return 0;

        return _considerations.Aggregate(1f, (current, c) => current * c.Score());
    }

    public abstract void Enter();
    public abstract void Execute(double elapsedMilliseconds);
    public abstract void Exit();

    public UtilityAiAction AddConsideration(IConsideration c)
    {
        ArgumentNullException.ThrowIfNull(c);
        _considerations.Add(c);
        return this;
    }
}