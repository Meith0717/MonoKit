// GameObjectFactory.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.Content;

namespace MonoKit.Gameplay;

public abstract class GameObjectFactory<T>
{
    // ReSharper disable once MemberCanBePrivate.Global
    protected readonly ContentContainer<object> ContentContainer;
    private readonly List<int> _ids = new();

    protected GameObjectFactory(ContentContainer<object> contentContainer)
    {
        ContentContainer = contentContainer;
        foreach (var (id, _, content) in contentContainer)
        {
            if (content is T data)
                _ids.Add(id);
        }
    }
}
