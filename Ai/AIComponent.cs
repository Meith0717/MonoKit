// AIComponent.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using MonoKit.Core.Utils;
using System.Collections.Generic;

namespace MonoKit.Ai
{
    public class AIComponent
    {
        private const float _evalInterval = 300f;
        private float _evalTimer = RNG.Random.Next((int)_evalInterval);

        private readonly List<IAIAction> _actions = [];
        private IAIAction _currentAction;

        public void AddAction(IAIAction action)
        {
            System.ArgumentNullException.ThrowIfNull(action);
            _actions.Add(action);
        }

        public void Update(double elapsedMilliseconds)
        {
            _evalTimer += (float)elapsedMilliseconds;

            if (_evalTimer >= _evalInterval)
            {
                _evalTimer = 0;
                EvaluateActions();
            }

            _currentAction?.Execute(elapsedMilliseconds);
        }

        private void EvaluateActions()
        {
            float bestScore = 0f;
            IAIAction best = null;

            foreach (var action in _actions)
            {
                float s = action.Evaluate();
                if (s > bestScore)
                {
                    bestScore = s;
                    best = action;
                }
            }

            if (best != _currentAction)
            {
                _currentAction?.Exit();
                _currentAction = best;
                _currentAction?.Enter();
            }
        }
    }

}
