// IAiAction.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

namespace MonoKit.Ai;

public interface IAiAction
{
    string Name { get; }

    float Evaluate();
    void Enter();
    void Execute(double elapsedMilliseconds);
    void Exit();
}
