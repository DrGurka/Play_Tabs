using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Play_Tabs.Tools;
using Microsoft.Xna.Framework.Input;

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
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Roundabout", album = "Fragile", artist = "Yes", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1979" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Africa", album = "Toto IV", artist = "Toto", length = 180, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1576" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Roundabout2", album = "Fragile", artist = "Yes", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1979" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Africa2", album = "Toto IV", artist = "Toto", length = 180, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1576" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Roundabout3", album = "Fragile", artist = "Yes", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1979" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Africa3", album = "Toto IV", artist = "Toto", length = 180, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1576" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Roundabout4", album = "Fragile", artist = "Yes", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1979" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Africa4", album = "Toto IV", artist = "Toto", length = 180, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1576" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Roundabout5", album = "Fragile", artist = "Yes", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1979" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Africa5", album = "Toto IV", artist = "Toto", length = 180, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1576" });
        }

        public void LoadContent(ContentManager Content)
        {
            font = Content.Load<SpriteFont>("font");
        }

        public void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
        }
        
        float target = 10;
        float distance;
        public void Draw(SpriteBatch spriteBatch)
        {
            int index = 0;
            distance += (target - distance) * 0.125f;
            Vector2 windowCenter = spriteBatch.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / 2f;
            foreach(SongObject song in SongOrganizer.songObjects)
            {
                //If the selection cursor is at the current index, highlight this song
                if (cursorIndex == index)
                {
                    //Song selection container
                    spriteBatch.Draw(rectangle, new Rectangle((int)((windowCenter.X - 640) + distance), 32 + (index * 128), 720, 128), null, Color.Black * 0.8f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);

                    //Detailed song info container
                    spriteBatch.Draw(rectangle, new Rectangle((int)((windowCenter.X - 640) + distance), 32 + (index * 128), 128, 128), null, Color.Black * 0.8f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                    spriteBatch.DrawString(font, song.title, new Vector2(windowCenter.X - 496 + distance, 48 + (index * 128)), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.2f);
                    spriteBatch.DrawString(font, song.artist, new Vector2(windowCenter.X - 496 + distance, 96 + (index * 128)), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                }

                else
                {
                    //Song selection container
                    spriteBatch.Draw(rectangle, new Rectangle((int)(windowCenter.X - 640), 32 + (index * 128), 720, 128), null, Color.Black * 0.5f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                    
                    //Detailed song info container
                    spriteBatch.Draw(rectangle, new Rectangle((int)(windowCenter.X - 640), 32 + (index * 128), 128, 128), null, Color.Black * 0.5f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1f);
                    spriteBatch.DrawString(font, song.title, new Vector2(windowCenter.X - 496, 48 + (index * 128)), Color.White, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0.2f);
                    spriteBatch.DrawString(font, song.artist, new Vector2(windowCenter.X - 496, 96 + (index * 128)), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                }
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

        //Use a timer for smooth scrolling in the song list
        double timer;
        float delay = 200;

        /// <summary>
        /// Manage user input
        /// </summary>
        /// <param name="gameTime"></param>
        public void HandleInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && (timer + delay) < gameTime.TotalGameTime.TotalMilliseconds)
            {
                timer = gameTime.TotalGameTime.TotalMilliseconds;
                delay = Math.Max(delay * 0.84f, 40); //Lower the delay of the timer if the user keeps scrolling
                cursorIndex--;

                if (cursorIndex < 0)
                    cursorIndex = (short)(SongOrganizer.songObjects.Count - 1);
                distance = 0;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) && (timer + delay) < gameTime.TotalGameTime.TotalMilliseconds)
            {
                timer = gameTime.TotalGameTime.TotalMilliseconds;
                delay = Math.Max(delay * 0.84f, 40); //Lower the delay of the timer if the user keeps scrolling
                cursorIndex++;

                if (cursorIndex > (short)(SongOrganizer.songObjects.Count - 1))
                    cursorIndex = 0;
                distance = 0;
            }

            //Reset delay if the user stops scrolling
            if (Keyboard.GetState().IsKeyUp(Keys.Up) && Keyboard.GetState().IsKeyUp(Keys.Down))
            {
                delay = 200;
            }
        }
    }
}
