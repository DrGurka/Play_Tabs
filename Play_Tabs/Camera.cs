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
    class Camera
    {
        public Vector3 position;
        public Vector3 target;

        public Matrix projectionMatrix;
        public Matrix viewMatrix;
        public Matrix worldMatrix;

        public Camera(GraphicsDevice graphicsDevice)
        {
            position = new Vector3(0, 0, -5);
            target = new Vector3(0, 0, 0);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), graphicsDevice.Viewport.AspectRatio, 1, 1000);
            viewMatrix = Matrix.CreateLookAt(position, target, new Vector3(0, 1, 0));
            worldMatrix = Matrix.CreateWorld(target, Vector3.Forward, Vector3.Up);
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                position.X += 1f;
                target.X += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                position.X -= 1f;
                target.X -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                position.Y += 1f;
                target.Y += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                position.Y -= 1f;
                target.Y -= 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
            {
                position.Z += 1f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
            {
                position.Z -= 1f;
            }

            viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
        }
    }
}
