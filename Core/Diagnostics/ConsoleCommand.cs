// ConsoleCommand.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;

namespace MonoKit.Core.Diagnostics;

public class ConsoleCommand(string name, string description, Action<string[]> execute)
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public Action<string[]> Execute { get; } = execute;
}
