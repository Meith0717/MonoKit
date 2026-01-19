// ConsoleManager.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace MonoKit.Core.Diagnostics;

public static class ConsoleManager
{
    // Only available on Windows — guard with OS checks
#if WINDOWS
    [DllImport("kernel32.dll")]
    private static extern bool AllocConsole();

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();
#endif

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

    public static void DrawProgressBar(string info, int progress, int total, int barWidth = 40)
    {
        if (Console.IsOutputRedirected)
            return;

        var percent = (double)progress / total;
        var filled = (int)(percent * barWidth);

        var bar = new string('█', filled) + new string(' ', barWidth - filled);

        Console.Write($"\r{info} [{bar}] {percent * 100:0.0}%");
    }

    public static void ClearLine()
    {
        if (Console.IsOutputRedirected)
            return;

        Console.Write("\033[2K");
        Console.Write("\r");
    }
}
