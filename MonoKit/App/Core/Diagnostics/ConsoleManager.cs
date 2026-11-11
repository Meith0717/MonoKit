// MonoKit - ConsoleManager.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;

namespace MonoKit.App.Core.Diagnostics;

public static class ConsoleManager
{
    // Only available on Windows — guard with OS checks
#if WINDOWS
    /// <summary>Allocates a new Win32 console window for the process.</summary>
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    /// <summary>Detaches the process from its current console window.</summary>
    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();
#endif

    /// <summary>Shows a console window and prints the welcome message.</summary>
    public static void Show(string welcomeMessage = "Console initialized.")
    {
#if WINDOWS
        // Only run if executed on Windows (prevents Linux errors)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                AllocConsole();
            }
            catch
            { /* ignore */
            }
        }
#endif
        Console.WriteLine(welcomeMessage + "\n");
    }

    /// <summary>Hides the console window on Windows.</summary>
    public static void Hide()
    {
#if WINDOWS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                FreeConsole();
            }
            catch
            { /* ignore */
            }
        }
#endif
    }

    /// <summary>Renders an inline progress bar with the current percentage.</summary>
    public static void DrawProgressBar(string info, int progress, int total, int barWidth = 40)
    {
        if (Console.IsOutputRedirected)
            return;

        var percent = (double)progress / total;
        var filled = (int)(percent * barWidth);

        var bar = new string('█', filled) + new string(' ', barWidth - filled);

        Console.Write($"\r{info} [{bar}] {percent * 100:0.0}%");
    }

    /// <summary>Clears the current console line using an ANSI escape.</summary>
    public static void ClearLine()
    {
        if (Console.IsOutputRedirected)
            return;

        Console.Write("\033[2K");
        Console.Write("\r");
    }
}
