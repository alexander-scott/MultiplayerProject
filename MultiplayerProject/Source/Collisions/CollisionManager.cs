using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MultiplayerProject.Source
{
    class CollisionManager
    {
        public enum CollisionType
        {
            LaserToPlayer,
            LaserToEnemy
        }

        public struct Collision
        {
            public CollisionType CollisionType { get; set; }
            public string LaserID { get; set; }
            public string AttackingPlayerID { get; set; }
            public string DefeatedPlayerID { get; set; } // Null if enemy was shot
            public string DefeatedEnemyID { get; set; } // Null if player was shot

            public Collision(CollisionType collisionType, string laserID, string attackingPlayerID, string defeatedPlayerID, string defeatedEnemyID)
            {
                CollisionType = collisionType;
                LaserID = laserID;
                AttackingPlayerID = attackingPlayerID;
                DefeatedPlayerID = defeatedPlayerID;
                DefeatedEnemyID = defeatedEnemyID;
            }
        }

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
            List<Collision> collisions = new List<Collision>();
            bool laserStillActive;

            for (int iLaser = 0; iLaser < lasers.Count; iLaser++) // Loop through every active laser
            {
                Rectangle laserRectangle = new Rectangle(
                    (int)lasers[iLaser].Position.X,
                    (int)lasers[iLaser].Position.Y,
                    lasers[iLaser].Width,
                    lasers[iLaser].Height);
                laserStillActive = true;

                for (int iPlayer = 0; iPlayer < players.Count; iPlayer++) // Loop through every active player
                {
                    Rectangle playerRectangle = new Rectangle(
                    (int)players[iPlayer].Position.X,
                    (int)players[iPlayer].Position.Y,
                    players[iPlayer].Width,
                    players[iPlayer].Height);

                    if (lasers[iLaser].PlayerFiredID != players[iPlayer].NetworkID // Make sure we don't check for collisions which the player that fired it
                        && laserRectangle.Intersects(playerRectangle)) 
                    {
                        collisions.Add(new Collision(CollisionType.LaserToPlayer, lasers[iLaser].LaserID, lasers[iLaser].PlayerFiredID, players[iPlayer].NetworkID, ""));
                        Console.WriteLine("SUCCESSFULL LASER/PLAYER INTERSECTION");
                        laserStillActive = false;
                        continue; // If collided don't check if collided with enemy as the laser has been destroyed
                    }

                }

                if (laserStillActive)
                {
                    for (int iEnemy = 0; iEnemy < enemies.Count; iEnemy++) // Loop through every enemy
                    {
                        Rectangle enemyRectangle = new Rectangle(
                        (int)enemies[iEnemy].Position.X,
                        (int)enemies[iEnemy].Position.Y,
                        enemies[iEnemy].Width,
                        enemies[iEnemy].Height);

                        if (laserRectangle.Intersects(enemyRectangle))
                        {
                            //collisions.Add(new Collision(CollisionType.LaserToEnemy, lasers[iLaser].LaserID, lasers[iLaser].PlayerFiredID, "", enemies[iEnemy].));
                        }
                    }
                }
            }


            return collisions;
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
