using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public class DisconnectedFromServerScene : IScene
    {
        private SpriteFont _font;
        private GraphicsDevice _device;

        public DisconnectedFromServerScene()
        {
        }

        public Client Client { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "DISCONNECTED FROM SERVER", new Vector2(161, 161), Color.Black);
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            _device = graphicsDevice;
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {

        }

        public void RecieveServerResponse(byte[] packet, MessageType type)
        {
            throw new NotImplementedException();
        }

        public void SendMessageToTheServer(BasePacket packet, MessageType messageType)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime gameTime)
        {

        }
    }
}
