// MonoKit - DebugConsole.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Threading;

namespace MonoKit.App.Core.Diagnostics;

/// <summary>
///     Interactive debug console: owns the command registry and the background input
///     thread. Start on app init, dispose on shutdown. Safe to call <see cref="Start" />
///     more than once.
/// </summary>
public sealed class DebugConsole : IDisposable
{
    private bool _disposed;
    private Thread _inputThread;
    private volatile bool _running;

    /// <summary>Command registry; register custom commands before <see cref="Start" />.</summary>
    public ConsoleCommands Commands { get; } = new();

    public bool IsRunning => _running;

    /// <summary>Stops the console and hides the window once.</summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Stop();
        ConsoleManager.Hide();
    }

    /// <summary>Shows the console and starts the background input thread.</summary>
    public void Start(string welcomeMessage = "Console initialized.")
    {
        if (_running || _disposed)
            return;

        ConsoleManager.Show(welcomeMessage);

        _running = true;
        _inputThread = new Thread(InputLoop) { IsBackground = true, Name = "DebugConsoleInput" };
        _inputThread.Start();
    }

    /// <summary>Reads console input lines and dispatches them as commands.</summary>
    private void InputLoop()
    {
        while (_running)
        {
            string? input;
            try
            {
                input = Console.ReadLine();
            }
            catch
            {
                break;
            }

            // null = EOF (input redirected/closed): exit instead of spinning.
            if (input == null)
                break;

            Commands.Execute(input);
        }
    }

    /// <summary>Signals the input loop to exit.</summary>
    public void Stop()
    {
        _running = false;
    }
}
