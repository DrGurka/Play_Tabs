using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Play_Tabs
{
    class Scene
    {
        List<Entity> entities = new List<Entity>();
        Camera camera;

        public Scene(GraphicsDevice graphicsDevice)
        {
            camera = new Camera(graphicsDevice);

        }

        public void LoadContent(ContentManager Content)
        {
            entities.Add(new Entity(0, camera, Content.Load<Model>("MonoCube"), new Vector3(0, 5, 0)));
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
