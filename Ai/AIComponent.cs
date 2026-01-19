// AIComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using MonoKit.Core.Utils;

namespace MonoKit.Ai;

public class AiComponent
{
    private const float EvalInterval = 300f;

    private readonly List<IAiAction> _actions = [];
    private IAiAction _currentAction;
    private float _evalTimer = RNG.Random.Next((int)EvalInterval);

    public void AddAction(IAiAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _actions.Add(action);
    }

    public void Update(double elapsedMilliseconds)
    {
        _evalTimer += (float)elapsedMilliseconds;

        if (_evalTimer >= EvalInterval)
        {
            _evalTimer = 0;
            EvaluateActions();
        }

        _currentAction?.Execute(elapsedMilliseconds);
    }

    private void EvaluateActions()
    {
        var bestScore = 0f;
        IAiAction best = null;

        foreach (var action in _actions)
        {
            var s = action.Evaluate();
            if (!(s > bestScore))
                continue;
            bestScore = s;
            best = action;
        }

        if (best == _currentAction)
            return;
        _currentAction?.Exit();
        _currentAction = best;
        _currentAction?.Enter();
    }
}
