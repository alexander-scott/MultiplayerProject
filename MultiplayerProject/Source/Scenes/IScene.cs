using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    interface IScene
    {
        int Width { get; set; }
        int Height { get; set; }

        void Initalise(ContentManager content, GraphicsDevice graphicsDevice);
        void Update(GameTime gameTime);
        void ProcessInput(GameTime gameTime, InputInformation inputInfo);
        void Draw(SpriteBatch spriteBatch);
    }
}
