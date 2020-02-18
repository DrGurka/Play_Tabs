using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Play_Tabs.Tools;

namespace Play_Tabs
{
    class Scene
    {
        List<Entity> entities = new List<Entity>();
        SpriteFont font;
        Camera camera;




        Vector2 textPos = Vector2.Zero;

        public Scene(GraphicsDevice graphicsDevice)
        {
            camera = new Camera(graphicsDevice);
            SongOrganizer.Initialize();
            SongOrganizer.songObjects.Add(new SongObject("") {title ="Roundabout", album = "Fragile", artist = "Yes", length = 300, tuningLead = new sbyte[6], tuningRhythm = new sbyte[6], year = "1979" });
            SongOrganizer.songObjects[0].GetLength();
        }

        public void LoadContent(ContentManager Content)
        {
            entities.Add(new Entity(0, camera, Content.Load<Model>("MonoCube"), new Vector3(0, 0, 0)));
            font = Content.Load<SpriteFont>("font");
        }

        public void Update(GameTime gameTime)
        {
            camera.Update(gameTime);

            foreach (Entity entity in entities)
            {
                entity.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Entity entity in entities)
            {
                entity.Draw(spriteBatch);
            }

            spriteBatch.Begin();
            textPos = Vector2.Zero;
            foreach (SongObject song in SongOrganizer.songObjects)
            {
                spriteBatch.DrawString(font, "Title: " + song.title, textPos, Color.White);
                //textPos.Y += 20;
                textPos.X += 200;
                spriteBatch.DrawString(font, "Artist: " + song.artist, textPos, Color.White);
                textPos.Y += 20;
                textPos.X = 0;
                spriteBatch.DrawString(font, "Album: " + song.album, textPos, Color.White);
                textPos.Y += 20;
                spriteBatch.DrawString(font, "Length: " + song.GetLength(), textPos, Color.White);
                textPos.Y += 20;
                spriteBatch.DrawString(font, "Year: " + song.year, textPos, Color.White);
                textPos.Y += 20;
            }
            spriteBatch.End();
        }

        /// <summary>
        /// Find all entities with a specific ID.
        /// </summary>
        /// <param name="id">The ID to find</param>
        /// <returns></returns>
        List<Entity> FindEntitiesById(ushort id)
        {
            return entities.FindAll(entity => entity.id == id);
        }
    }
}
