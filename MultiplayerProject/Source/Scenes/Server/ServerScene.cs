using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        private const int MAX_ROWS = 10;

        private List<string> logLines;
        private string _path;
        private float yGap;

        public ServerScene(int width, int height)
        {
            Width = width;
            Height = height;

            _path = Assembly.GetExecutingAssembly().GetName().Name + ".log";

            bool b;
            logLines = Logger.Instance.ReadLastLines(0, 10, out b);

            Logger.OnNewLogItem += Logger_OnNewLogItem;
        }

        private void Logger_OnNewLogItem(string str)
        {
            logLines.Insert(0, str);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int count = 0;
            for (int i = 0; i < logLines.Count; i++)
            {
                if (logLines.Count > i && logLines[i] != null && !string.IsNullOrEmpty(logLines[i]))
                {
                    spriteBatch.DrawString(_font, logLines[i], new Vector2(0, 50 + (count * yGap)), Color.Black);
                    count++;
                }
            }
            
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            yGap = _font.MeasureString("T").Y;
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
