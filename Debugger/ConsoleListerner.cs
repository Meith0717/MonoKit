// ConsoleListerner.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Threading;

namespace MonoKit.Debug
{
    public class ConsoleListerner(ConsoleCommands commands)
    {
        private readonly ConsoleCommands _commands = commands;
        private Thread _inputThread;
        private bool _running = false;

        public void Start()
        {
            _running = true;
            _inputThread = new Thread(InputLoop)
            {
                IsBackground = true,
                Name = "ConsoleInputThread"
            };
            _inputThread.Start();
        }

        private void InputLoop()
        {
            while (_running)
            {
                string? input = Console.ReadLine();
                if (input == null) 
                    continue;

                _commands.Execute(input);
            }
        }


        public void Stop()
        {
            _running = false;
            try
            {
                if (Console.In.Peek() == -1)
                    Console.WriteLine();
            }
            catch { /* ignore */ }
        }

    }
}
