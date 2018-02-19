using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class ExplosionManager
    {
        // Collections of explosions
        List<Explosion> explosions;

        //Texture to hold explosion animation.
        Texture2D explosionTexture;

        public void Initalise(ContentManager content)
        {
            // init our collection of explosions.
            explosions = new List<Explosion>();

            // load the explosion sheet
            explosionTexture = content.Load<Texture2D>("explosion");
        }

        public void Update(GameTime gameTime)
        {
            for (var e = 0; e < explosions.Count; e++)
            {
                explosions[e].Update(gameTime);

                if (!explosions[e].Active)
                    explosions.Remove(explosions[e]);
            }
        }

        public void AddExplosion(Vector2 enemyPosition)
        {
            Animation explosionAnimation = new Animation();

            explosionAnimation.Initialize(explosionTexture,
                enemyPosition,
                134,
                134,
                12,
                30,
                Color.White,
                1.0f,
                true);

            Explosion explosion = new Explosion();
            explosion.Initialize(explosionAnimation, enemyPosition);

            explosions.Add(explosion);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw explosions
            foreach (var e in explosions)
            {
                e.Draw(spriteBatch);
            }
        }
    }
}
