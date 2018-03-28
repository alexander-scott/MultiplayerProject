using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class ExplosionManager
    {
        // Collections of explosions
        private List<Explosion> _explosions;

        //Texture to hold explosion animation.
        private Texture2D _explosionTexture;

        public void Initalise(ContentManager content)
        {
            // init our collection of explosions.
            _explosions = new List<Explosion>();

            // load the explosion sheet
            _explosionTexture = content.Load<Texture2D>("explosion");
        }

        public void Update(GameTime gameTime)
        {
            for (var e = 0; e < _explosions.Count; e++)
            {
                _explosions[e].Update(gameTime);

                if (!_explosions[e].Active)
                    _explosions.Remove(_explosions[e]);
            }
        }

        public void AddExplosion(Vector2 enemyPosition)
        {
            Animation explosionAnimation = new Animation();

            explosionAnimation.Initialize(_explosionTexture,
                enemyPosition,
                0,
                134,
                134,
                12,
                30,
                Color.White,
                1.0f,
                true);

            Explosion explosion = new Explosion();
            explosion.Initialize(explosionAnimation, enemyPosition);

            _explosions.Add(explosion);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw explosions
            for(int i = 0; i < _explosions.Count; i++)
            {
                _explosions[i].Draw(spriteBatch);
            }
        }
    }
}
