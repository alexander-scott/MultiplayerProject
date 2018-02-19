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

                // If the speed has the background moving to the left
                if (Speed <= 0)
                {
                    // Check the texture is out of view then put that texture at the end of the screen
                    if (Positions[i].X <= -Texture.Width)
                    {
                        Positions[i].X = Texture.Width * (Positions.Length - 1);
                    }
                }
                // If the speed has the background moving to the right
                else
                {
                    // Check if the texture is out of view then position it to the start of the screen
                    if (Positions[i].X >= Texture.Width * (Positions.Length - 1))
                    {
                        Positions[i].X = -Texture.Width;
                    }
                }
            }
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
