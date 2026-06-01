// GameObjectFactory.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using MonoKit.Content;

namespace MonoKit.Gameplay;

public abstract class GameObjectFactory<T>(ContentContainer<object> contentProvider)
    where T : class
{
    public readonly ContentContainer<T> ContentProvider = contentProvider.Cast<T>();
}
