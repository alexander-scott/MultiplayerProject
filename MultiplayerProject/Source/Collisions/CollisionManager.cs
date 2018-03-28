using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class CollisionManager
    {
        public void CheckCollision(List<Enemy> enemies, List<Laser> lasers, ExplosionManager explosionManager)
        {
            Rectangle rectangle2;
            Rectangle laserRectangle;

            // detect collisions between the player and all enemies.
            enemies.ForEach(e =>
            {
                //create a retangle for the enemy
                rectangle2 = new Rectangle(
                    (int)e.CentrePosition.X,
                    (int)e.CentrePosition.Y,
                    e.Width,
                    e.Height);

                // now see if this enemy collide with any laser shots
                lasers.ForEach(lb =>
                {
                    // create a rectangle for this laserbeam
                    laserRectangle = new Rectangle(
                    (int)lb.Position.X,
                    (int)lb.Position.Y,
                    lb.Width,
                    lb.Height);

                    // test the bounds of the laer and enemy
                    if (laserRectangle.Intersects(rectangle2))
                    {
                        // play the sound of explosion.
                        //var explosion = explosionSound.CreateInstance();
                        //explosion.Play();

                        explosionManager.AddExplosion(lb.Position);

                        // kill off the enemy
                        e.Health = 0;

                        //record the kill
                        //myGame.Stage.EnemiesKilled++;

                        // kill off the laserbeam
                        lb.Active = false;

                        // record your score
                        //myGame.Score += e.Value;
                    }
                });
            });
        }

        public List<Collision> CheckCollision(List<Player> players, List<Enemy> enemies, List<Laser> lasers)
        {
            return null;

        }

        public void Draw(GraphicsDevice device, SpriteBatch spriteBatch, List<Enemy> enemies, List<Laser> lasers)
        {
            foreach (Enemy enemy in enemies)
            {
                Texture2D texture = new Texture2D(device, enemy.Width, enemy.Height);
                texture.CreateBorder(1, Color.Red);
                spriteBatch.Draw(texture, enemy.CentrePosition, Color.White);
            }

            foreach (Laser laser in lasers)
            {
                Texture2D texture = new Texture2D(device, laser.Width, laser.Height);
                texture.CreateBorder(1, Color.Blue);
                spriteBatch.Draw(texture, laser.Position, Color.White);
            }
        }
    }

    static class Utilities
    {
        public static void CreateBorder(this Texture2D texture, int borderWidth, Color borderColor)
        {
            Color[] colors = new Color[texture.Width * texture.Height];

            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    bool colored = false;
                    for (int i = 0; i <= borderWidth; i++)
                    {
                        if (x == i || y == i || x == texture.Width - 1 - i || y == texture.Height - 1 - i)
                        {
                            colors[x + y * texture.Width] = borderColor;
                            colored = true;
                            break;
                        }
                    }

                    if (colored == false)
                        colors[x + y * texture.Width] = Color.Transparent;
                }
            }

            texture.SetData(colors);
        }
    }
}
