// IAiAction.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace MonoKit.Ai;

public interface IAiAction
{
    string Name { get; }

    float Evaluate();
    void Enter();
    void Execute(double elapsedMilliseconds);
    void Exit();
}
