using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    public class WaitingRoomScene : IScene
    {
        public int Width { get; set; }
        public int Height { get; set; }

        private WaitingRoomInformation _waitingRoom;
        private SpriteFont _font;

        public WaitingRoomScene(int width, int height)
        {
            Width = width;
            Height = height;

            Client.OnWaitingRoomInformationRecieved += Client_OnWaitingRoomInformationRecieved;
        }

        private void Client_OnWaitingRoomInformationRecieved(WaitingRoomInformation waitingRoom)
        {
            _waitingRoom = waitingRoom;
        }

        public void Initalise(ContentManager content)
        {
            _font = content.Load<SpriteFont>("Font");
        }

        public void ProcessInput(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, MouseState mouseState)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "Tesyt", new Vector2(161, 161), Color.Black);
        }
    }
}
