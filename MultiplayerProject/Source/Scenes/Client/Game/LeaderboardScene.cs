using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerProject.Source
{
    public class LeaderboardScene : IScene
    {
        private SpriteFont _font;
        private int _width;
        private int _height;

        public LeaderboardScene(int width, int height, LeaderboardPacket leaderboardPacket)
        {
            _width = width;
            _height = height;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "LEADERBOARDS", new Vector2(161, 161), Color.Black);
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            
        }
    }
}
