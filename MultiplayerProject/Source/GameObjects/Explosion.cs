using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class Explosion
    {
        public int Width
        {
            get { return _explosionAnimation.FrameWidth; }
        }

        public int Height
        {
            get { return _explosionAnimation.FrameWidth; }
        }

        public bool Active;

        private Animation _explosionAnimation;
        private Vector2 _position;      
        private int _timeToLive;

        public void Initialize(Animation animation, Vector2 position)
        {
            _explosionAnimation = animation;
            _position = position;
            _timeToLive = 30;

            Active = true;
        }

        public void Update(GameTime gameTime)
        {
            _explosionAnimation.Update(gameTime);

            _timeToLive -= 1;

            if (_timeToLive <= 0)
            {
                Active = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _explosionAnimation.Draw(spriteBatch);
        }
    }
}
