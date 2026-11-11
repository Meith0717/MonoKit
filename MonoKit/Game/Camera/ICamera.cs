// MonoKit - ICamera.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using Microsoft.Xna.Framework;
using MonoKit.App.Input;

namespace MonoKit.Game.Camera;

/// <summary>Dimension-agnostic camera: world position, view matrix, per-frame update.</summary>
public interface ICamera
{
    /// <summary>World position the camera looks at. 2D cameras use X/Y only.</summary>
    Vector3 Position { get; set; }

    /// <summary>World-to-screen view matrix.</summary>
    Matrix View { get; }

    /// <summary>Advances the camera state using input and elapsed time.</summary>
    void Update(double elapsedGameTime, InputHandler inputHandler);
}
