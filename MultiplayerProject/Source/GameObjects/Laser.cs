using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    class Laser
    {
        // animation the represents the laser animation.
        public Animation LaserAnimation;

        // position of the laser
        public Vector2 Position;

        public float Rotation;

        // set the laser to active
        public bool Active;

        // the width of the laser image.
        public int Width
        {
            get { return LaserAnimation.FrameWidth; }
        }

        // the height of the laser image.
        public int Height
        {
            get { return LaserAnimation.FrameHeight; }
        }

        // the speed the laser travels
        private float _laserMoveSpeed = 30f;

        // The damage the laser deals.
        //private int _laserDamage = 10;

        // Laser beams range.
        //private int _laserRange;

        public void Initialize(Animation animation, Vector2 position, float rotation)
        {
            LaserAnimation = animation;
            Position = position;
            Rotation = rotation;
            Active = true;
        }

        public void Update(GameTime gameTime)
        {
            Vector2 direction = new Vector2((float)Math.Cos(Rotation),
                                     (float)Math.Sin(Rotation));
            direction.Normalize();
            Position += direction * _laserMoveSpeed;

            LaserAnimation.Position = Position;
            LaserAnimation.Rotation = Rotation;
            LaserAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            LaserAnimation.Draw(spriteBatch);
        }
    }
}
