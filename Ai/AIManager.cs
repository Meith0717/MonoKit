// AIManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;

namespace MonoKit.Ai;

public class AiManager
{
    private readonly List<AiComponent> _agents = [];

    public void Register(AiComponent ai)
    {
        _agents.Add(ai);
    }

    public void Unregister(AiComponent ai)
    {
        _agents.Remove(ai);
    }

    public void Update(double elapsedMilliseconds)
    {
        foreach (var ai in _agents)
            ai.Update(elapsedMilliseconds);
    }
}
