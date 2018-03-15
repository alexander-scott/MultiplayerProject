using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MultiplayerProject.Source
{
    interface IScene
    {
        int Width { get; }
        int Height { get; }

        void Initalise(ContentManager content);
        void Update(GameTime gameTime);
        void ProcessInput(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState, MouseState mouseState);
        void Draw(SpriteBatch spriteBatch);
    }
}
