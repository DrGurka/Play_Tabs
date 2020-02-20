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
    class GameHandle
    {
        private List<Entity> entities;
        private Dictionary<string, Model> modelList;
        public GameHandle()
        {
            modelList = new Dictionary<string, Model>();
            entities = new List<Entity>();
        }

        public void LoadContent(ContentManager Content)
        {
            modelList.Add("MonoCube", Content.Load<Model>("MonoCube"));
        }

        public void Update(GameTime gameTime)
        {
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
    }
}
