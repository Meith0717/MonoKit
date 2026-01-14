// AIManager.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

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
