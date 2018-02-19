using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class ParallaxingBackground
    {
        // The image representing the parallaxing background
        Texture2D Texture;

        // An array of positions of the parallaxing background
        Vector2[] Positions;

        // The speed which the background is moving
        int Speed;

        int bgHeight;
        int bgWidth;

        public void Initialize(ContentManager content, String texturePath, int screenWidth, int screenHeight, int speed)
        {
            bgHeight = screenHeight;
            bgWidth = screenWidth;

            // Load the background texture we will be using
            Texture = content.Load<Texture2D>(texturePath);

            // Set the speed of the background
            Speed = speed;

            // If we divide the screen with the texture width then we can determine the number of tiles need.
            // We add 1 to it so that we won't have a gap in the tiling
            Positions = new Vector2[screenWidth / Texture.Width + 1];

            // Set the initial positions of the parallaxing background
            for (int i = 0; i < Positions.Length; i++)
            {
                // We need the tiles to be side by side to create a tiling effect
                Positions[i] = new Vector2(i * Texture.Width, 0);
            }
        }

        public void Update(GameTime gametime)
        {
            // Update the positions of the background
            for (int i = 0; i < Positions.Length; i++)
            {
                // Update the position of the screen by adding the speed
                Positions[i].X += Speed;
            }

            for (int i = 0; i < Positions.Length; i++)
            {
                if (Speed <= 0)
                {
                    // Check if the texture is out of view and then put that texture at the end of the screen.
                    if (Positions[i].X <= -Texture.Width)
                    {
                        WrapTextureToLeft(i);
                    }
                }
                else
                {
                    if (Positions[i].X >= Texture.Width * (Positions.Length - 1))
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
                prevTexture = Positions.Length - 1;

            Positions[index].X = Positions[prevTexture].X + Texture.Width;
        }

        private void WrapTextureToRight(int index)
        {
            // If the textures are scrolling to the right, when the tile wraps, it should be placed to the left
            // of the tile that comes after it.
            int nextTexture = index + 1;
            if (nextTexture == Positions.Length)
                nextTexture = 0;

            Positions[index].X = Positions[nextTexture].X - Texture.Width;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < Positions.Length; i++)
            {
                Rectangle rectBg = new Rectangle((int)Positions[i].X, (int)Positions[i].Y, bgWidth, bgHeight);
                spriteBatch.Draw(Texture, rectBg, Color.White);
            }
        }
    }
}
