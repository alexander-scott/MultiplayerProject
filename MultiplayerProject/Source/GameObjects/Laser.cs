using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class Laser : INetworkedObject
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

        public string NetworkID { get; set; }

        public string LaserID { get; set; }

        // the speed the laser travels
        private const float _laserMoveSpeed = 30f;
        private const float _laserMaxTimeActive = 5f;

        private float _timeActive;

        public Laser()
        {
            LaserID = Guid.NewGuid().ToString();
        }

        public Laser(string ID)
        {
            LaserID = ID;
        }

        public void Initialize(Animation animation, Vector2 position, float rotation)
        {
            _timeActive = 0;
            LaserAnimation = animation;
            Position = position;
            Rotation = rotation;
            Active = true;
        }

        public void Update(GameTime gameTime)
        {
            Update((float)gameTime.ElapsedGameTime.TotalSeconds);

            LaserAnimation.Position = Position;
            LaserAnimation.Rotation = Rotation;
            LaserAnimation.Update(gameTime);
        }

        public void Update(float deltaTime)
        {
            Vector2 direction = new Vector2((float)Math.Cos(Rotation),
                                     (float)Math.Sin(Rotation));
            direction.Normalize();
            Position += direction * _laserMoveSpeed;

            _timeActive += deltaTime;

            if (_timeActive > _laserMaxTimeActive)
            {
                Active = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            LaserAnimation.Draw(spriteBatch);
        }
    }
}
