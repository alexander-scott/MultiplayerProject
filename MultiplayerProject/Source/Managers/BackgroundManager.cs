using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class BackgroundManager
    {
        // Image used to display the static background
        private Texture2D _mainBackground;
        private Rectangle _rectBackground;

        private ParallaxingBackground _bgLayer1;
        private ParallaxingBackground _bgLayer2;

        private int _screenWidth;
        private int _screenHeight;

        public BackgroundManager(int screenWidth, int screenHeight)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
        }

        public void Initalise(ContentManager content)
        {
            _bgLayer1 = new ParallaxingBackground();
            _bgLayer2 = new ParallaxingBackground();
            _rectBackground = new Rectangle(0, 0, _screenWidth, _screenHeight);

            _bgLayer1.Initialize(content, "bgLayer1", _screenWidth, _screenHeight, -1);
            _bgLayer2.Initialize(content, "bgLayer2", _screenWidth, _screenHeight, -2);
            _mainBackground = content.Load<Texture2D>("mainbackground");
        }

        public void Update(GameTime gameTime)
        {
            // Update the parallaxing background
            _bgLayer1.Update(gameTime);
            _bgLayer2.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //Draw the Main Background Texture
            spriteBatch.Draw(_mainBackground, _rectBackground, Color.White);

            // Draw the moving background
            _bgLayer1.Draw(spriteBatch);
            _bgLayer2.Draw(spriteBatch);
        }
    }
}
