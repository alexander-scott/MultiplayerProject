using Microsoft.Xna.Framework;
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
                    (int)e.Position.X,
                    (int)e.Position.Y,
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
    }
}
