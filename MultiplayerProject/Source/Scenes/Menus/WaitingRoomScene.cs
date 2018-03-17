using System;
using System.Collections.Generic;
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
        private List<Rectangle> _lobbyRectangles;

        private SpriteFont _font;
        private GraphicsDevice _device;

        public WaitingRoomScene(int width, int height)
        {
            Width = width;
            Height = height;

            _lobbyRectangles = new List<Rectangle>();

            Client.OnWaitingRoomInformationRecieved += Client_OnWaitingRoomInformationRecieved;
        }

        private void Client_OnWaitingRoomInformationRecieved(WaitingRoomInformation waitingRoom)
        {
            _waitingRoom = waitingRoom;
            _lobbyRectangles.Clear();
            if (_waitingRoom != null)
            {
                int startYPos = 0;
                foreach (var lobby in _waitingRoom.Lobbies)
                {
                    Rectangle boundaries = new Rectangle(50, startYPos, 500, 50);
                    _lobbyRectangles.Add(boundaries);
                    startYPos += 50;
                }
            }
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            _device = graphicsDevice;
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            if (inputInfo.CurrentMouseState.LeftButton == ButtonState.Pressed && inputInfo.PreviousMouseState.LeftButton == ButtonState.Released)
            {
                // TODO: Change to check for a click not if mouse is held/pressed
                for (int i = 0; i < _lobbyRectangles.Count; i++)
                {
                    if (_lobbyRectangles[i].Contains(inputInfo.CurrentMouseState.Position))
                    {
                        
                    }
                }
            } 
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_waitingRoom != null)
            {
                for (int i = 0; i < _waitingRoom.Lobbies.Length; i++)
                {
                    string displayString = _waitingRoom.Lobbies[i].LobbyName + " : " + _waitingRoom.Lobbies[i].ConnectionCount + "/"+ WaitingRoom.MAX_PEOPLE_PER_LOBBY + " Players";
                    Vector2 size = _font.MeasureString(displayString);

                    float xScale = (_lobbyRectangles[i].Width / size.X);
                    float yScale = (_lobbyRectangles[i].Height / size.Y);

                    // Taking the smaller scaling value will result in the text always fitting in the boundaires.
                    float scale = Math.Min(xScale, yScale);

                    // Figure out the location to absolutely-center it in the boundaries rectangle.
                    int strWidth = (int)Math.Round(size.X * scale);
                    int strHeight = (int)Math.Round(size.Y * scale);
                    Vector2 position = new Vector2();
                    position.X = (((_lobbyRectangles[i].Width - strWidth) / 2) + _lobbyRectangles[i].X);
                    position.Y = (((_lobbyRectangles[i].Height - strHeight) / 2) + _lobbyRectangles[i].Y);

                    // A bunch of settings where we just want to use reasonable defaults.
                    float rotation = 0.0f;
                    Vector2 spriteOrigin = new Vector2(0, 0);
                    float spriteLayer = 0.0f; // all the way in the front
                    SpriteEffects spriteEffects = new SpriteEffects();

                    // Draw the string to the sprite batch!
                    spriteBatch.DrawString(_font, displayString, position, Color.White, rotation, spriteOrigin, scale, spriteEffects, spriteLayer);

                    Texture2D texture = new Texture2D(_device, _lobbyRectangles[i].Width, _lobbyRectangles[i].Height);
                    texture.CreateBorder(1, Color.Blue);
                    spriteBatch.Draw(texture, position, Color.White);
                }
            }
        }
    }
}
