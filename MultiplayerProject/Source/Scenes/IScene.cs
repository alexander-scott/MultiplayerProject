using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    public interface IScene
    {
        Client Client { get;set; }

        void Initalise(ContentManager content, GraphicsDevice graphicsDevice);
        void Update(GameTime gameTime);
        void ProcessInput(GameTime gameTime, InputInformation inputInfo);
        void Draw(SpriteBatch spriteBatch);

        void RecieveServerResponse(byte[] packet, MessageType type);
        void SendMessageToTheServer(BasePacket packet, MessageType messageType);
    }
}
