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

        private const float TEXT_SCALE = 0.5f;
        private const float START_Y_POS = 50f;

        // Title
        private Vector2 _titlePosition;
        private const string _titleText = "SERVER LOG";

        private List<string> logLines;
        private string _path;
        private float yGap;

        public ServerScene(int width, int height)
        {
            Width = width;
            Height = height;

            _path = Assembly.GetExecutingAssembly().GetName().Name + ".log";

            bool b;
            logLines = Logger.Instance.ReadLastLines(0, 30, out b);

            Logger.OnNewLogItem += Logger_OnNewLogItem;
        }

        private void Logger_OnNewLogItem(string str)
        {
            logLines.Insert(0, str);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw title
            spriteBatch.DrawString(_font, _titleText, _titlePosition, Color.White);

            int count = 0;
            for (int i = 0; i < logLines.Count; i++)
            {
                if (logLines.Count > i && logLines[i] != null && !string.IsNullOrEmpty(logLines[i]))
                {
                    try
                    {
                        spriteBatch.DrawString(_font, logLines[i], new Vector2(0, START_Y_POS + (count * yGap)), Color.Black, 0, Vector2.Zero, TEXT_SCALE, SpriteEffects.None, 0);
                        count++;
                    }
                    catch { /* This is bad */ }
                }
            }          
        }

        public void Initalise(ContentManager content, GraphicsDevice graphicsDevice)
        {
            _font = content.Load<SpriteFont>("Font");
            yGap = _font.MeasureString("T").Y * TEXT_SCALE;
            _device = graphicsDevice;

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
