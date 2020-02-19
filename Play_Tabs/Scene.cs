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

        public Camera camera;
        public Dictionary<string, Model> modelList;
        private MenuHandle menuHandle;

        public Scene(GraphicsDevice graphicsDevice)
        {
            camera = new Camera(graphicsDevice);
            modelList = new Dictionary<string, Model>();
            menuHandle = new MenuHandle(graphicsDevice);
        }

        public void LoadContent(ContentManager Content)
        {

            modelList.Add("MonoCube", Content.Load<Model>("MonoCube"));

            entities.Add(new Entity(0, camera, modelList["MonoCube"], new Vector3(0, 0, 0)));

            menuHandle.LoadContent(Content);
        }

        public void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
            menuHandle.Update(gameTime);

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
            menuHandle.Draw(spriteBatch);
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
