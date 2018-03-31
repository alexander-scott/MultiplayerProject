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
    public class ServerScene
    {
        private SpriteFont _font;
        private GraphicsDevice _device;

        public int Width { get; set; }
        public int Height { get; set; }

        public ServerScene(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, "YOU ARE THE SERVER", new Vector2(161, 161), Color.Black);
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            _device = graphicsDevice;
        }

        public void ProcessInput(GameTime gameTime, InputInformation inputInfo)
        {
            
        }

        public void Update(GameTime gameTime)
        {
            
        }
    }
}
