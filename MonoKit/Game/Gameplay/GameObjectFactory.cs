// MonoKit - GameObjectFactory.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.App.Content;

namespace MonoKit.Game.Gameplay;

public abstract class GameObjectFactory<T>(ContentContainer<object> contentProvider)
    where T : class
{
    public readonly ContentContainer<T> ContentProvider = contentProvider.Cast<T>();
}
