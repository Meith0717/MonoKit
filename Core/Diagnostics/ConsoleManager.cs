// ConsoleManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace MonoKit.Core.Diagnostics
{
    public static class ConsoleManager
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static void Show(string welcomeMessage = "Console initialized.")
        {
            // AllocConsole(); Only works on Windows
            Console.WriteLine(welcomeMessage + "\n");
        }

        public static void Hide()
        {
            // FreeConsole(); Only works on windows
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

            var currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

    }
}
