// ConsoleCommands.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MonoKit.Core.Diagnostics;

public class ConsoleCommands
{
    private readonly Dictionary<string, ConsoleCommand> _commands = new();

    public ConsoleCommands()
    {
        Register(new ConsoleCommand("help", "Lists all commands", args => PrintHelp()));
    }

    public void Register(ConsoleCommand command)
    {
        _commands[command.Name.ToLower()] = command;
    }

    public bool Execute(string input)
    {
        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;

        var name = parts[0].ToLower();
        var args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(name, out var command))
            try
            {
                command.Execute(args);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing '{name}': {ex.Message}");
                return false;
            }

        Console.WriteLine($"Unknown command '{name}'. Type 'help' for a list of commands.");
        return false;
    }

    public void PrintHelp()
    {
        Console.WriteLine("Available commands:");
        foreach (var cmd in _commands.Values)
            Console.WriteLine($"  {cmd.Name} - {cmd.Description}");
    }
}