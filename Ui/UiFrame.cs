// UiFrame.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using MonoKit.Content;
using MonoKit.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;

namespace MonoKit.Ui
{
    public class UiFrame() : UiElement
    {
        private readonly List<UiElement> _elementChilds = new();
        private Texture2D _texture;
        private float _textureScale = 1;
        private Color _color = Color.White;

        public Color Color { set => _color = value; }

        protected override void Updater(InputHandler inputHandler)
        {
            UpdateSizeIfTextureNotNull();
            _elementChilds.RemoveAll(c => c.IsDisposed);
            foreach (var child in _elementChilds)
                child.Update(inputHandler, Bounds, UiScale);
        }

        protected override void Drawer(SpriteBatch spriteBatch)
        {
            if (_texture is not null)
                spriteBatch.Draw(_texture, Bounds, _color);
            else if (_color != Color.Transparent)
                spriteBatch.FillRectangle(Bounds, _color);

            foreach (var child in _elementChilds)
                child.Draw(spriteBatch);
        }

        public override void ApplyScale(Rectangle root, float uiScale)
        {
            base.ApplyScale(root, uiScale);
            foreach (var child in _elementChilds)
                child.ApplyScale(Bounds, uiScale);
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var child in _elementChilds)
                child.Dispose();
        }

        public void Clear()
        {
            _elementChilds.Clear();
        }

        public void Add(UiElement child)
        {
            _elementChilds.Add(child);
        }

        public UiFrame AttachTexture(string texture, float scale = 1)
        {
            _texture = ContentProvider.Textures.Get(texture);
            _textureScale = scale;
            return this;
        }

        private void UpdateSizeIfTextureNotNull()
        {
            if (_texture is null) return;
            Width = (int)(_texture.Width * _textureScale);
            Height = (int)(_texture.Height * _textureScale);
        }
    }
}
