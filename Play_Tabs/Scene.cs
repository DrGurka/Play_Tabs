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
        

        public Camera camera;
        public Dictionary<string, Model> modelList;
        private MenuHandle menuHandle;
        private GameHandle gameHandle;

        public Scene(GraphicsDevice graphicsDevice)
        {
            camera = new Camera(graphicsDevice);
            modelList = new Dictionary<string, Model>();
            menuHandle = new MenuHandle(graphicsDevice);
            gameHandle = new GameHandle();
        }

        public void LoadContent(ContentManager Content)
        {

            gameHandle.LoadContent(Content);
            menuHandle.LoadContent(Content);
        }

        public void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
            menuHandle.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            gameHandle.Draw(spriteBatch);
            spriteBatch.Begin();
            menuHandle.Draw(spriteBatch);
            spriteBatch.End();
        }
    }
}
