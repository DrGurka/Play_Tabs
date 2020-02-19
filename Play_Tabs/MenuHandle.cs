using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Play_Tabs.Tools;

namespace Play_Tabs
{
    class MenuHandle
    {

        private Texture2D rectangle;
        private SpriteFont font;
        private short cursorIndex;

        public MenuHandle(GraphicsDevice graphicsDevice)
        {

            rectangle = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            rectangle.SetData<Color>(new Color[] { Color.White });

            SongOrganizer.Initialize(graphicsDevice);

            //Test
            //SongOrganizer.songObjects.Add(new SongObject("") { title = "Roundabout", album = "Fragile", artist = "Yes", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1979" });
            //SongOrganizer.songObjects[0].GetLength();
        }

        public void LoadContent(ContentManager Content)
        {
            font = Content.Load<SpriteFont>("font");
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            
            int index = 0;
            Vector2 windowCenter = spriteBatch.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / 2f;
            foreach(SongObject song in SongOrganizer.songObjects)
            {
                spriteBatch.Draw(rectangle, new Rectangle((int)(windowCenter.X - 640), 32 + (index * 128), 720, 128), null, Color.Black * 0.5f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                //Album art temp
                spriteBatch.Draw(rectangle, new Rectangle((int)(windowCenter.X - 640), 32 + (index * 128), 128, 128), null, Color.Black * 0.5f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                spriteBatch.DrawString(font, song.title, new Vector2(windowCenter.X - 496, 48 + (index * 128)), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(font, song.artist, new Vector2(windowCenter.X - 496, 96 + (index * 128)), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                index++;
            }

            spriteBatch.Draw(rectangle, new Rectangle((int)(windowCenter.X + 96), 32, 300, 600), null, Color.Black * 0.5f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);

            if (SongOrganizer.songObjects.Count > 0)
            {
                SongObject currentSong = SongOrganizer.songObjects[cursorIndex];
                if (currentSong.isLoaded && currentSong.images[0].isLoaded) {
                    spriteBatch.Draw(currentSong.images[0].image, new Vector2((int)(windowCenter.X + 96), 32), null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.1f);
                }
                spriteBatch.DrawString(font, "SONG INFO", new Vector2((int)(windowCenter.X + 96) + 16, 32 + 300 + 16), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(font, "Artist: " + currentSong.artist, new Vector2((int)(windowCenter.X + 96) + 16, 32 + 300 + 16 + 64), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(font, "Album: " + currentSong.album, new Vector2((int)(windowCenter.X + 96) + 16, 32 + 300 + 16 + 96), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(font, "Year: " + currentSong.year, new Vector2((int)(windowCenter.X + 96) + 16, 32 + 300 + 16 + 128), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(font, "Title: " + currentSong.title, new Vector2((int)(windowCenter.X + 96) + 16, 32 + 300 + 16 + 160), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(font, "Length: " + currentSong.GetLength(), new Vector2((int)(windowCenter.X + 96) + 16, 32 + 300 + 16 + 192), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
            }
        }
    }
}
