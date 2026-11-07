// MonoKit - ConsoleCommand.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;

namespace MonoKit.App.Core.Diagnostics;

public class ConsoleCommand(string name, string description, Action<string[]> execute)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public Action<string[]> Execute { get; } = execute;
}
