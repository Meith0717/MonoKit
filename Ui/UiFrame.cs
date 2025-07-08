// UiFrame.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using GameEngine.Content;
using GameEngine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace GameEngine.Ui
{
    public class UiFrame() : UiElement
    {
        private readonly List<UiElement> _elementChilds = new();
        private readonly RenderTarget2D _renderTarget;

        public Color Color { private get; set; } = Color.White;
        public float Alpha { private get; set; } = 1;
        public string Texture { private get; set; }
        public float TextureScale { private get; set; } = 1;

        public void Clear()
            => _elementChilds.Clear();

        public void Add(UiElement child)
        {
            _elementChilds.Add(child);
            child.ApplyScale(Bounds, UiScale);
        }

        protected override void Updater(InputState inputState)
        {
            UpdateSizeIfTextureNotNull();
            foreach (UiElement child in _elementChilds)
                child.Update(inputState, Bounds);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            DrawTextureOrRectangle(spriteBatch);
            foreach (UiElement child in _elementChilds)
                child.Draw(spriteBatch);
        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            _renderTarget?.Dispose();

            foreach (UiElement child in _elementChilds)
                child.ApplyScale(Bounds, uiScale);
        }

        private void DrawTextureOrRectangle(SpriteBatch spriteBatch)
        {
            if (Texture is not null)
                spriteBatch.Draw(ContentProvider.Textures.Get(Texture), Bounds, Color * Alpha);
            else if (Alpha != 0 && Color != Color.Transparent)
                spriteBatch.FillRectangle(Bounds, Color * Alpha);
        }

        private void UpdateSizeIfTextureNotNull()
        {
            if (Texture is null) return;
            var texture = ContentProvider.Textures.Get(Texture);
            Width = (int)(texture.Width * TextureScale);
            Height = (int)(texture.Height * TextureScale);
        }
    }
}
