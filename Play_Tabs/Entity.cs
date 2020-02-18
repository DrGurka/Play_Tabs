using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Play_Tabs
{
    class Entity
    {
        public readonly ushort id;
        readonly Camera camera;
        Model model;

        Vector3 position;

        public Entity(ushort id, Camera camera, Model model, Vector3 position)
        {
            this.id = id;
            this.camera = camera;
            this.model = model;
            this.position = position;
        }



        public virtual void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                position.X += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                position.X -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                position.Y += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                position.Y -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                position.Z += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                position.Z -= 1f;
            }
        }



        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach(var mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.AmbientLightColor = new Vector3(1, 0, 0);

                    effect.View = camera.viewMatrix;
                    effect.World = camera.worldMatrix * Matrix.CreateTranslation(position);
                    effect.Projection = camera.projectionMatrix;
                }

                mesh.Draw();
            }
        }
    }
}
