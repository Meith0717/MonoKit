# 🧩 MonoKit

> A lightweight and modular game framework built on **MonoGame**.  
> MonoKit provides essential building blocks for 2D games — including content management, rendering, input handling, and
> UI components.

MonoKit is **not a complete game engine** — it’s a collection of building blocks meant to help you start small and learn
how engines are structured.  
It focuses on:

- Simplicity over performance
- Readability over abstraction
- Modularity without overengineering

---

## 🌟 Overview

**MonoKit** is a simple game development library built on top of **MonoGame**, created for learning and small to medium
2D projects.  
It offers a structured foundation without being a full-blown engine, making it easy to understand, extend, and
customize.

### ✨ Features

- **App host** — a composition root that wires content, audio, input, display, and screens together
- **Content system** — boot + incremental loading, typed content containers, settings and path management
- **Rendering** — screen compositing with an optional post-processing effect (e.g. blur behind menus)
- **Input** — keyboard, mouse, and gamepad listeners with configurable action bindings and OS text input
- **Gameplay** — an archetype-based ECS (AECS) with parallel iteration and spatial neighbor queries
- **UI toolkit** — buttons, checkboxes, sliders, text entries, boxes, tab views, and windows
- **Camera system** — 2D and 3D cameras behind a dimension-agnostic interface
- **Runtime** — a screen stack plus a `GameHost` that bundles world and camera for 2D or 3D

---

## 🏗️ Architecture

The library is split into two layers, each in its own namespace:

```text
MonoKit
├── MonoKit.App          Application layer
│   ├── AppHost          Composition root: wires all services into the MonoGame game instance
│   ├── Content          ContentProvider, ContentLoader, ContentLayout, SettingsManager, PathService
│   │   └── Audio        MusicManager, SoundEffectManager
│   ├── Core             File/JSON/IO helpers, frame counter
│   │   └── Diagnostics  In-game debug console and commands
│   ├── Display          Window mode, resolution, viewport scaling
│   ├── Input            Keyboard/mouse/gamepad listeners + action bindings
│   ├── Rendering        Post-processing runner for screen compositing
│   ├── Screens          Screen stack with update/draw separation
│   └── Ui               UiButton, UiSlider, UiText, UiVBox, UiTabview, UiWindow, ...
│
└── MonoKit.Game         Gameplay layer
    ├── ArchetypeEcs     Archetype-based ECS: entities, archetypes, worlds, spatial queries
    ├── Camera           Camera2D / Camera3D behind ICamera
    ├── Components       Transform2D/3D, Velocity2D/3D, Collider2D/3D, SpriteRenderer, Lifetime
    ├── Systems          LifetimeSystem, MovementSystem2D/3D, SpriteDrawSystem2D
    ├── Gameplay         GameObjectFactory, RuntimeContainer
    ├── Screens          GameScreen — hosts a GameHost inside the app's screen stack
    └── GameHost         Bundles an AECS world + camera for a 2D or 3D runtime
```

### 🧬 Archetype ECS (AECS)

The heart of the gameplay layer is a small archetype-based entity component system:

- Entities are plain IDs; components are `struct`s stored column-wise per archetype
- `World.ForEach<T1..T4>` iterates matching components by `ref`, with parallel variants
- Structural changes made during iteration are buffered as commands and flushed afterwards
- `SpatialQueryContext<TPos>` provides radius-based neighbor queries with frame-stable indices

### 🚀 Quick start

```bash
# Build the library
dotnet build MonoKit.sln

# Run the demo application
dotnet run --project MonoKit.Demo

# Run the tests
dotnet test MonoKit.Test
```

### 🎮 Demo application

`MonoKit.Demo` shows the full stack in action: a loading screen with incremental content loading,
a main menu, a gameplay screen running the AECS world, pause menu, settings screen, and a UI
playground with all widgets.

---

## 💬 Development approach

MonoKit is a personal learning project. I used **Mistral Vibe** as my AI coding companion — mainly to
understand engine and ECS concepts and to review ideas — but the code itself is written and adapted by
me. Every piece of the library was hand-written to actually understand how it works, which is the whole
point of this project.

---

## 📄 License

This project is licensed under the Creative Commons **Attribution-NonCommercial 4.0 International**
license — see [LICENCE](LICENCE).
