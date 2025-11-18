// AIManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;

namespace MonoKit.Ai
{
    public class AIManager
    {
        private readonly List<AIComponent> agents = [];

        public void Register(AIComponent ai) => agents.Add(ai);
        public void Unregister(AIComponent ai) => agents.Remove(ai);

        public void Update(double elapsedMilliseconds)
        {
            foreach (var ai in agents)
                ai.Update(elapsedMilliseconds);
        }
    }

}
