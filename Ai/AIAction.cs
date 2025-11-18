// AIAction.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;

namespace MonoKit.Ai
{
    public class AIAction(string name, Action onEnter, Action<double> onExecute, Action onExit) : IAIAction
    {
        private readonly List<Consideration> considerations = [];
        private readonly Action onEnter = onEnter;
        private readonly Action<double> onExecute = onExecute;
        private readonly Action onExit = onExit;

        public string Name { get; } = name;

        public AIAction AddConsideration(Consideration c)
        {
            System.ArgumentNullException.ThrowIfNull(c);
            considerations.Add(c);
            return this;
        }

        public float Evaluate()
        {
            if (considerations.Count == 0)
                return 0;

            float score = 1f;
            foreach (var c in considerations)
                score *= c.Score();

            return score;
        }

        public void Enter() => onEnter?.Invoke();
        public void Execute(double elapsedMilliseconds) => onExecute?.Invoke(elapsedMilliseconds);
        public void Exit() => onExit?.Invoke();
    }

}
