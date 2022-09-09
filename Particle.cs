using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;

namespace TankGame
{
    class Particle
    {
        Model particleModel;
        BasicEffect effect;
        Matrix worldMatrix;

        public Vector3 position;
        public Vector3 speed;
        public Vector3 acceleration = new Vector3(0f, -9.8f, 0f);
        Random rand;

        bool isAlive = false;
        float exitSpeed = 5f;
        float scale;

        public Particle(GraphicsDevice device, ContentManager content, Vector3 position, Vector3 speed)
        {

            this.speed = speed;
            particleModel = content.Load<Model>("dust");
            scale = 0.02f;
            effect = new BasicEffect(device);
            float aspectRatio = (float)device.Viewport.Width / device.Viewport.Height;
            effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 1.0f, 3.0f), Vector3.Zero, Vector3.Up);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.01f, 10.0f);
            this.position = position;
            worldMatrix = Matrix.Identity;
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
            rand = new Random();

        }
        public void Update(GameTime gameTime)
        {
            if (isAlive == false)
            {
                isAlive = true;
                speed.Normalize();
                speed = speed * exitSpeed;
            }
            if (isAlive)
            {
                speed += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                position += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
        }
        public void Draw(GraphicsDevice device, Camera camara)
        {
            if (isAlive == true)
            {
                foreach (ModelMesh mesh in particleModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                        effect.View = camara.viewMatrix;
                        effect.Projection = camara.projectionMatrix;

                        effect.EnableDefaultLighting();
                    }
                    //Draw each mesh of the model
                    mesh.Draw();
                }
            }
        }
    }
}