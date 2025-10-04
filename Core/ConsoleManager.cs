// ConsoleManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace GameEngine.Core
{
    public static class ConsoleManager
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public static void Show()
        {
            AllocConsole();
            Console.WriteLine("Console initialized.\n");
        }

        public static void Hide()
        {
            FreeConsole();
        }
    }
}
