using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class BackgroundManager
    {
        // Image used to display the static background
        Texture2D mainBackground;
        Rectangle rectBackground;

        ParallaxingBackground bgLayer1;
        ParallaxingBackground bgLayer2;

        private int _screenWidth;
        private int _screenHeight;

        public void Initalise(ContentManager content, int screenWidth, int screenHeight)
        {
            bgLayer1 = new ParallaxingBackground();
            bgLayer2 = new ParallaxingBackground();
            rectBackground = new Rectangle(0, 0, screenWidth, screenHeight);

            bgLayer1.Initialize(content, "bgLayer1", screenWidth, screenHeight, -1);
            bgLayer2.Initialize(content, "bgLayer2", screenWidth, screenHeight, -2);
            mainBackground = content.Load<Texture2D>("mainbackground");

            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public void Update(GameTime gameTime)
        {
            // Update the parallaxing background
            bgLayer1.Update(gameTime);
            bgLayer2.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Draw the Main Background Texture
            spriteBatch.Draw(mainBackground, rectBackground, Color.White);

            // Draw the moving background
            bgLayer1.Draw(spriteBatch);
            bgLayer2.Draw(spriteBatch);
        }
    }
}
