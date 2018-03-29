using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    public class LaserManager : INetworkedObject
    {
        public List<Laser> Lasers { get { return _laserBeams; } }

        private List<Laser> _laserBeams;
        private Dictionary<string, List<Laser>> _playerLasers;

        // texture to hold the laser.
        private Texture2D _laserTexture;

        // govern how fast our laser can fire.
        private TimeSpan _laserSpawnTime;
        private TimeSpan _previousLaserSpawnTime;

        private const float SECONDS_IN_MINUTE = 60f;
        private const float RATE_OF_FIRE = 200f;
        private const float LASER_SPAWN_DISTANCE = 40f;

        public string NetworkID { get; set; }

        public LaserManager()
        {
            // Init our laser
            _laserBeams = new List<Laser>();
            _playerLasers = new Dictionary<string, List<Laser>>();

            _laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / RATE_OF_FIRE);
            _previousLaserSpawnTime = TimeSpan.Zero;
        }

        public void Initalise(ContentManager content)
        {
            // Load the texture to serve as the laser
            _laserTexture = content.Load<Texture2D>("laser");
        }

        public void Update(GameTime gameTime)
        {
            // Update laserbeams
            for (var i = 0; i < _laserBeams.Count; i++)
            {
                _laserBeams[i].Update(gameTime);
                // Remove the beam when its deactivated or is at the end of the screen.
                if (!_laserBeams[i].Active)
                {
                    _laserBeams.Remove(_laserBeams[i]);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the lasers.
            for (int i = 0; i < _laserBeams.Count; i++)
            {
                _laserBeams[i].Draw(spriteBatch);
            }
        }

        public Laser FireLocalLaserClient(GameTime gameTime, Vector2 position, float rotation, PlayerColour colour)
        {
            // Govern the rate of fire for our lasers
            if (gameTime.TotalGameTime - _previousLaserSpawnTime > _laserSpawnTime)
            {
                _previousLaserSpawnTime = gameTime.TotalGameTime;
                // Add the laer to our list.
                return AddLaser(position, rotation, "", "", colour);
            }

            return null;
        }

        public void FireRemoteLaserClient(Vector2 position, float rotation, string playerID, DateTime originalTimeFired, string laserID, PlayerColour colour)
        {
            var timeDifference = (originalTimeFired - DateTime.UtcNow).TotalSeconds;

            var laser = AddLaser(position, rotation, laserID, playerID, colour);
            laser.Update((float)timeDifference); // Update it to match the true position

            if (!_playerLasers.ContainsKey(playerID))
            {
                _playerLasers.Add(playerID, new List<Laser>());
            }

            _playerLasers[playerID].Add(laser);
        }

        public Laser FireLaserServer(double totalGameSeconds, float deltaTime, Vector2 position, float rotation, string laserID, string playerFiredID)
        {
            // Govern the rate of fire for our lasers
            if (totalGameSeconds - _previousLaserSpawnTime.TotalSeconds > _laserSpawnTime.TotalSeconds)
            {
                _previousLaserSpawnTime = TimeSpan.FromSeconds(totalGameSeconds);

                // Add the laser to our list.
                var laser = AddLaser(position, rotation, laserID, playerFiredID, new PlayerColour(255, 255, 255));
                laser.Update(deltaTime); // Update it so it's in the correct position on the server as it is on the client

                return laser;
            }

            return null;
        }

        public Laser AddLaser(Vector2 position, float rotation, string laserID, string playerFiredID, PlayerColour colour)
        {
            Animation laserAnimation = new Animation();
            // Initlize the laser animation
            laserAnimation.Initialize(_laserTexture,
                position,
                rotation,
                46,
                16,
                1,
                30,
                new Color(colour.R, colour.G, colour.B),
                1f,
                true);

            Laser laser;
            if (string.IsNullOrEmpty(laserID))
                laser = new Laser();
            else
                laser = new Laser(laserID, playerFiredID);

            Vector2 direction = new Vector2((float)Math.Cos(rotation),
                                     (float)Math.Sin(rotation));
            direction.Normalize();

            // Move the starting position to be slightly in front of the cannon
            var laserPostion = position;
            laserPostion += direction * LASER_SPAWN_DISTANCE;

            // Init the laser
            laser.Initialize(laserAnimation, laserPostion, rotation);
            _laserBeams.Add(laser);

            return laser;
        }

        public void DeactivateLaser(string laserID)
        {
            for (int i = 0; i < _laserBeams.Count; i++)
            {
                if (_laserBeams[i].LaserID == laserID)
                {
                    _laserBeams[i].Active = false;
                    return;
                }
            }
        }
    }
}
