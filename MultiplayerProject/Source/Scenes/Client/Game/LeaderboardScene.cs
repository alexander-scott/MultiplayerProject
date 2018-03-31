using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    public enum LeaderboardSceneState
    {
        WaitingForInput,
        RematchSelected,
    }

    public class LeaderboardScene : IScene
    {
        private SpriteFont _font;
        private GraphicsDevice _device;
        private int _width;
        private int _height;
        private LeaderboardPacket _leaderboard;

        private LeaderboardSceneState _sceneState;

        private string _titleText = "LEADERBOARD";
        private Vector2 _titlePosition;

        // Rematch button
        private Vector2 _rematchButtonPosition;
        private Rectangle _rematchButtonRect;
        private Color _rematchButtonColour;
        private string _rematchButtonText = "RESTART GAME";

        // Back to waiting room button
        private Vector2 _exitButtonPosition;
        private Rectangle _exitButtonRect;
        private Color _exitButtonColour;
        private string _exitButtonText = "RETURN TO MAIN MENU";

        public LeaderboardScene(int width, int height, LeaderboardPacket leaderboardPacket)
        {
            _width = width;
            _height = height;
            _leaderboard = leaderboardPacket;
            _sceneState = LeaderboardSceneState.WaitingForInput;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            int startYPos = 50;
            int startXPos = (_width / 2) - 200;

            // DRAW NAMES
            for (int i = 0; i < _leaderboard.PlayerCount; i++)
            {
                spriteBatch.DrawString(_font, _leaderboard.PlayerNames[i], new Vector2(startXPos, startYPos), new Color(_leaderboard.PlayerColours[i].R, _leaderboard.PlayerColours[i].G, _leaderboard.PlayerColours[i].B));
                startYPos += 50;
            }

            // DRAW SCORES
            startYPos = 50;
            startXPos = (_width / 2) + 100;
            for (int i = 0; i < _leaderboard.PlayerCount; i++)
            {
                spriteBatch.DrawString(_font, _leaderboard.PlayerScores[i].ToString(), new Vector2(startXPos, startYPos), new Color(_leaderboard.PlayerColours[i].R, _leaderboard.PlayerColours[i].G, _leaderboard.PlayerColours[i].B));
                startYPos += 50;
            }

            // Draw rematch button
            spriteBatch.DrawString(_font, _rematchButtonText, _rematchButtonPosition, Color.White);
            Texture2D rematchBtnTexture = new Texture2D(_device, _rematchButtonRect.Width, _rematchButtonRect.Height);
            rematchBtnTexture.CreateBorder(1, _rematchButtonColour);
            spriteBatch.Draw(rematchBtnTexture, _rematchButtonPosition, Color.White);

            // Draw exit button
            spriteBatch.DrawString(_font, _exitButtonText, _exitButtonPosition, Color.White);
            Texture2D exitBtnTexture = new Texture2D(_device, _exitButtonRect.Width, _exitButtonRect.Height);
            exitBtnTexture.CreateBorder(1, _exitButtonColour);
            spriteBatch.Draw(exitBtnTexture, _exitButtonPosition, Color.White);
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _device = graphicsDevice;
            _font = content.Load<SpriteFont>("Font");

            _titlePosition = new Vector2(Application.WINDOW_WIDTH / 2, 0);
            _titlePosition.X -= (_font.MeasureString(_titleText).X / 2);

            ReformatButtons();
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (_sceneState == LeaderboardSceneState.WaitingForInput)
            {
                if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_rematchButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        // READY
                        _sceneState = LeaderboardSceneState.RematchSelected;
                    }
                    else if (_exitButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        // LEAVE GAME
                    }
                }
                else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // If hover
                {
                    if (_rematchButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        _rematchButtonColour = Color.LightGreen;
                    }
                    else
                    {
                        _rematchButtonColour = Color.Blue;
                    }

                    if (_exitButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        _exitButtonColour = Color.LightGreen;
                    }
                    else
                    {
                        _exitButtonColour = Color.Blue;
                    }
                }
            }
            else
            {
                _exitButtonColour = Color.Red;

                // If clicked
                if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
                {
                    if (_rematchButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        // UNREADY
                        _sceneState = LeaderboardSceneState.WaitingForInput;
                    }
                }
                else if (inputInfo.PreviousMouseState.LeftButton == ButtonState.Released) // If hover
                {
                    if (_rematchButtonRect.Contains(inputInfo.CurrentMouseState.Position))
                    {
                        _rematchButtonColour = Color.Orange;
                    }
                    else
                    {
                        _rematchButtonColour = Color.LightGreen;
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            
        }

        private void ReformatButtons()
        {
            _exitButtonPosition = new Vector2(Application.WINDOW_WIDTH / 2, Application.WINDOW_HEIGHT);
            _exitButtonPosition.Y -= (_font.MeasureString(_exitButtonText).Y);
            _exitButtonPosition.X -= (_font.MeasureString(_exitButtonText).X) / 2;
            _exitButtonRect = new Rectangle((int)_exitButtonPosition.X, (int)_exitButtonPosition.Y,
                (int)_font.MeasureString(_exitButtonText).X, (int)_font.MeasureString(_exitButtonText).Y);

            _rematchButtonPosition = new Vector2(Application.WINDOW_WIDTH / 2, Application.WINDOW_HEIGHT);
            _rematchButtonPosition.Y -= (_font.MeasureString(_rematchButtonText).Y) * 2;
            _rematchButtonPosition.X -= (_font.MeasureString(_rematchButtonText).X) / 2;
            _rematchButtonRect = new Rectangle((int)_rematchButtonPosition.X, (int)_rematchButtonPosition.Y,
                (int)_font.MeasureString(_rematchButtonText).X, (int)_font.MeasureString(_rematchButtonText).Y);
        }
    }
}
