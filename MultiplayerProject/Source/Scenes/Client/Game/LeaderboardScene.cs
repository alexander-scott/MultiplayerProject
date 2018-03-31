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
        private LeaderboardPacket _leaderboard;

        private string _titleText = "LEADERBOARD";
        private Vector2 _titlePosition;

        public LeaderboardScene(int width, int height, LeaderboardPacket leaderboardPacket)
        {
            _width = width;
            _height = height;
            _leaderboard = leaderboardPacket;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            int startYPos = 100;
            int startXPos = (_width / 2) - 200;

            // DRAW NAMES
            for (int i = 0; i < _leaderboard.PlayerCount; i++)
            {
                spriteBatch.DrawString(_font, _leaderboard.PlayerNames[i], new Vector2(startXPos, startYPos), new Color(_leaderboard.PlayerColours[i].R, _leaderboard.PlayerColours[i].G, _leaderboard.PlayerColours[i].B));
                startYPos += 50;
            }

            // DRAW SCORES
            startYPos = 100;
            startXPos = (_width / 2) + 100;
            for (int i = 0; i < _leaderboard.PlayerCount; i++)
            {
                spriteBatch.DrawString(_font, _leaderboard.PlayerScores[i].ToString(), new Vector2(startXPos, startYPos), new Color(_leaderboard.PlayerColours[i].R, _leaderboard.PlayerColours[i].G, _leaderboard.PlayerColours[i].B));
                startYPos += 50;
            }
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");

            _titlePosition = new Vector2(Application.WINDOW_WIDTH / 2, 0);
            _titlePosition.X -= (_font.MeasureString(_titleText).X / 2);
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            
        }
    }
}
