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
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Models;

namespace Play_Tabs
{
    class MenuHandle
    {
        private Texture2D rectangle;
        private Texture2D screenGradient;
        private SpriteFont fontRegular;
        private SpriteFont fontBold;
        private short cursorIndex;

        public MenuHandle(GraphicsDevice graphicsDevice)
        {
            rectangle = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            rectangle.SetData<Color>(new Color[] { Color.White });

            screenGradient = CreateGradient(graphicsDevice, new Color(49, 29, 63), new Color(32, 26, 44));

            SongOrganizer.Initialize(graphicsDevice);

            #region Test
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Roundabout", album = "Yesstory", artist = "Yes", length = 512, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1992" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Africa", album = "Toto IV", artist = "Toto", length = 296, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1982" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Parabola", album = "Lateralus", artist = "Tool", length = 364, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "2001" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "The Frail", album = "The Fragile", artist = "Nine Inch Nails", length = 114, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1999" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "The Outsider", album = "Thirteenth Step", artist = "A Perfect Circle", length = 246, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "2003" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Bonneville", album = "Malina", artist = "Leprous", length = 329, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "2017" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Amsterdam", album = "Broken Machine", artist = "Nothing But Thieves", length = 272, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "2017" });
            SongOrganizer.songObjects.Add(new SongObject("") { title = "Change (In the House of Flies)", album = "White Pony", artist = "Deftones", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "2000" });

            CredentialsAuth auth = new CredentialsAuth("e1c3ce28971a4396bd46eba97d14c271", "951448c26c5544ce8af238bdda3277d8");
            Token token = Task.Run(() => auth.GetToken()).Result;
            SpotifyWebAPI spotify = new SpotifyWebAPI()
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };

            foreach(SongObject song in SongOrganizer.songObjects)
            {
                SongOrganizer.albumImages.Add(song.artist + "+" + song.album, new AlbumImage(spotify, song.artist + "+" + song.album, graphicsDevice));
            }
            #endregion
        }

        private Texture2D CreateGradient(GraphicsDevice graphics, Color startColor, Color endColor)
        {
            double angle = MathHelper.ToRadians(45);
            double rx = Math.Cos(angle);
            double ry = Math.Sin(angle);
            Random random = new Random();

            double start = -1.0f * (rx + ry);
            double end = rx + ry;

            Texture2D gradient = new Texture2D(graphics, graphics.Viewport.Width, graphics.Viewport.Height);
            Color[] colorData = new Color[gradient.Width * gradient.Height];
            for(int x = 0; x < gradient.Width; x++)
            {
                for(int y = 0; y < gradient.Height; y++)
                {
                    double u = (x / (double)gradient.Width) * 2.0f - 1.0f;
                    double v = (y / (double)gradient.Height) * 2.0f - 1.0f;

                    double here = u * rx + v * ry;
                    colorData[x + (y * gradient.Width)] = Color.Lerp(startColor, endColor, (float)((start - here) / (start - end)) + (float)random.NextDouble() * 0.05f);
                }
            }

            gradient.SetData(colorData);
            return gradient;
        }
        public void LoadContent(ContentManager Content)
        {
            fontRegular = Content.Load<SpriteFont>("Fonts/Gravity-Regular");
            fontBold = Content.Load<SpriteFont>("Fonts/Gravity-Bold");
        }

        public void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
        }
        
        float target = 10;
        float distance;
        
        public void Draw(SpriteBatch spriteBatch)
        {

            spriteBatch.Draw(screenGradient, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);

            int index = 0;
            int songOffset = 132;
            distance += (target - distance) * 0.125f;
            Vector2 windowCenter = spriteBatch.GraphicsDevice.Viewport.Bounds.Size.ToVector2() / 2f;
            int offsetX = spriteBatch.GraphicsDevice.Viewport.X; //Alltid noll
            byte maxContainersVert = (byte)Math.Floor((windowCenter.Y / songOffset) - 0.5f);
            int alignCenter = (int)(windowCenter.Y - 64) - Math.Max((maxContainersVert - cursorIndex) * songOffset, 0) + Math.Max(cursorIndex - (SongOrganizer.songObjects.Count - 1 - maxContainersVert), 0) * songOffset;

            foreach(SongObject song in SongOrganizer.songObjects)
            {
                //If the selection cursor is at the current index, highlight this song
                if (cursorIndex == index)
                {
                    //Song selection container
                    spriteBatch.Draw(rectangle, new Rectangle((int)(offsetX + (distance * 1.5f)), alignCenter + (index * songOffset - cursorIndex * songOffset), 720, 128), null, Color.Black * 0.8f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                    spriteBatch.Draw(rectangle, new Rectangle(offsetX, alignCenter + (index * songOffset - cursorIndex * songOffset), (int)distance, 128), null, new Color(226, 62, 87), 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f); //Green highlight accent (#1DB954)

                    //Detailed song info container
                    song.DrawCoverArt(spriteBatch, new Vector2(offsetX + (distance * 1.5f), alignCenter + (index * songOffset - cursorIndex * songOffset)), false, 0.2f);
                    spriteBatch.DrawString(fontBold, song.title, new Vector2(offsetX + 144 + distance, alignCenter + 16 + (index * songOffset - cursorIndex * songOffset)), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                    spriteBatch.DrawString(fontRegular, song.artist, new Vector2(offsetX + 144 + distance, alignCenter + 64 + (index * songOffset - cursorIndex * songOffset)), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                }

                else
                {
                    //Song selection container
                    spriteBatch.Draw(rectangle, new Rectangle(offsetX, alignCenter + (index * songOffset - cursorIndex * songOffset), 720, 128), null, Color.Black * 0.5f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                    
                    //Detailed song info container
                    song.DrawCoverArt(spriteBatch, new Vector2(offsetX, alignCenter + (index * songOffset - cursorIndex * songOffset)), false, 0.2f);
                    spriteBatch.DrawString(fontBold, song.title, new Vector2(offsetX + 144, alignCenter + 16 + (index * songOffset - cursorIndex * songOffset)), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                    spriteBatch.DrawString(fontRegular, song.artist, new Vector2(offsetX + 144, alignCenter + 64 + (index * songOffset - cursorIndex * songOffset)), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                }
                index++;
            }

            float offsetY = windowCenter.Y - 300;
            offsetX = 720 + ((spriteBatch.GraphicsDevice.Viewport.Width - 720) / 2) - 150;

            if (SongOrganizer.songObjects.Count > 0)
            {
                spriteBatch.Draw(rectangle, new Rectangle(offsetX, (int)offsetY, 300, 600), null, Color.Black * 0.5f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
                SongObject currentSong = SongOrganizer.songObjects[cursorIndex];
                currentSong.DrawCoverArt(spriteBatch, new Vector2(offsetX, offsetY), true, 0.3f);
                spriteBatch.DrawString(fontBold, "INFO", new Vector2(offsetX + 16, offsetY + 300 + 16), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(fontRegular, "Artist: " + currentSong.artist, new Vector2(offsetX + 16, offsetY + 300 + 16 + 64), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(fontRegular, "Album: " + currentSong.album, new Vector2(offsetX + 16, offsetY + 300 + 16 + 96), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(fontRegular, "Year: " + currentSong.year, new Vector2(offsetX + 16, offsetY + 300 + 16 + 128), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(fontRegular, "Title: " + currentSong.title, new Vector2(offsetX + 16, offsetY + 300 + 16 + 160), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
                spriteBatch.DrawString(fontRegular, "Length: " + currentSong.GetLength(), new Vector2(offsetX + 16, offsetY + 300 + 16 + 192), Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.2f);
            }
        }

        //Use a timer for smooth scrolling in the song list
        double timer;
        float delay = 400;

        /// <summary>
        /// Manage user input
        /// </summary>
        /// <param name="gameTime"></param>
        public void HandleInput(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Up) && (timer + delay) < gameTime.TotalGameTime.TotalMilliseconds)
            {
                timer = gameTime.TotalGameTime.TotalMilliseconds;
                delay = Math.Max(delay * 0.8f, 60); //Lower the delay of the timer if the user keeps scrolling
                cursorIndex--;

                if (cursorIndex < 0)
                    cursorIndex = (short)(SongOrganizer.songObjects.Count - 1);
                distance = 0; //Reset the distance of the highlighting animation
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down) && (timer + delay) < gameTime.TotalGameTime.TotalMilliseconds)
            {
                timer = gameTime.TotalGameTime.TotalMilliseconds;
                delay = Math.Max(delay * 0.8f, 60); //Lower the delay of the timer if the user keeps scrolling
                cursorIndex++;

                if (cursorIndex > (short)(SongOrganizer.songObjects.Count - 1))
                    cursorIndex = 0;
                distance = 0; //Reset the distance of the highlighting animation
            }

            //Reset delay if the user stops scrolling
            if (Keyboard.GetState().IsKeyUp(Keys.Up) && Keyboard.GetState().IsKeyUp(Keys.Down))
            {
                delay = 400;
                timer = 0;
            }
        }
    }
}
