using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class ParallaxingBackground
    {
        // The image representing the parallaxing background
        private Texture2D _texture;

        // An array of positions of the parallaxing background
        private Vector2[] _positions;

        // The speed which the background is moving
        private int _speed;

        private int _bgHeight;
        private int _bgWidth;

        public void Initialize(ContentManager content, String texturePath, int screenWidth, int screenHeight, int speed)
        {
            _bgHeight = screenHeight;
            _bgWidth = screenWidth;

            // Load the background texture we will be using
            _texture = content.Load<Texture2D>(texturePath);

            // Set the speed of the background
            _speed = speed;

            // If we divide the screen with the texture width then we can determine the number of tiles need.
            // We add 1 to it so that we won't have a gap in the tiling
            _positions = new Vector2[screenWidth / _texture.Width + 1];

            // Set the initial positions of the parallaxing background
            for (int i = 0; i < _positions.Length; i++)
            {
                // We need the tiles to be side by side to create a tiling effect
                _positions[i] = new Vector2(i * _texture.Width, 0);
            }
        }

        public void Update(GameTime gametime)
        {
            // Update the positions of the background
            for (int i = 0; i < _positions.Length; i++)
            {
                // Update the position of the screen by adding the speed
                _positions[i].X += _speed;
            }

            for (int i = 0; i < _positions.Length; i++)
            {
                if (_speed <= 0)
                {
                    // Check if the texture is out of view and then put that texture at the end of the screen.
                    if (_positions[i].X <= -_texture.Width)
                    {
                        WrapTextureToLeft(i);
                    }
                }
                else
                {
                    if (_positions[i].X >= _texture.Width * (_positions.Length - 1))
                    {
                        WrapTextureToRight(i);
                    }
                }
            }
        }

        private void WrapTextureToLeft(int index)
        {
            // If the textures are scrolling to the left, when the tile wraps, it should be put at the
            // one pixel to the right of the tile before it.
            int prevTexture = index - 1;
            if (prevTexture < 0)
                prevTexture = _positions.Length - 1;

            _positions[index].X = _positions[prevTexture].X + _texture.Width;
        }

        private void WrapTextureToRight(int index)
        {
            // If the textures are scrolling to the right, when the tile wraps, it should be placed to the left
            // of the tile that comes after it.
            int nextTexture = index + 1;
            if (nextTexture == _positions.Length)
                nextTexture = 0;

            _positions[index].X = _positions[nextTexture].X - _texture.Width;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < _positions.Length; i++)
            {
                Rectangle rectBg = new Rectangle((int)_positions[i].X, (int)_positions[i].Y, _bgWidth, _bgHeight);
                spriteBatch.Draw(_texture, rectBg, Color.White);
            }
        }
    }
}
