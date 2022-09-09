using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TankGame
{
    class Bullet
    {
        Model bulletModel;
        BasicEffect effect;
        Matrix worldMatrix;

        public Vector3 position;
        public Vector3 speed;
        public Vector3 acceleration = new Vector3(0f, -9.8f, 0f);
        Vector3 distance;

        bool isAlive = false;
        float exitSpeed = 25f;
        float scale;
        float bulletCollisionBoxRadius = 0.5f;

        public Bullet(GraphicsDevice device, ContentManager content)
        {
            bulletModel = content.Load<Model>("bullet");
            scale = 0.05f;
            // Vamos usar um efeito básico
            effect = new BasicEffect(device);
            // Calcula a aspectRatio, a view matrix e a projeção
            float aspectRatio = (float)device.Viewport.Width / device.Viewport.Height;
            effect.View = Matrix.CreateLookAt(new Vector3(0.0f, 1.0f, 3.0f), Vector3.Zero, Vector3.Up);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.01f, 10.0f);
            worldMatrix = Matrix.Identity;
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = true;
        }

        public void Update(GameTime gameTime, KeyboardState kb, Terrain terrain, Tank tank, EnemyTank enemyTank)
        {
            if (kb.IsKeyDown(Keys.Space))
            {
                isAlive = true;
                position = tank.boneTransforms[10].Translation;
                speed = tank.boneTransforms[10].Backward;
                speed.Normalize();
                speed = speed * exitSpeed;
            }

            if (isAlive)
            {
                distance = position - enemyTank.position;
                speed += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                position += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (position.Y <= terrain.GetHeight(position.X, position.Z) && position.X > terrain.terrainWidth - 1 || position.X < 0 || position.Z > terrain.terrainHeight - 1 || position.Z < 0 || distance.Length() < 2.0f * bulletCollisionBoxRadius)
                {
                    isAlive = false;
                }
            }
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {

            if (isAlive == true)
            {
                foreach (ModelMesh mesh in bulletModel.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {   
                        effect.World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                        effect.View = camera.viewMatrix;
                        effect.Projection = camera.projectionMatrix;

                        effect.EnableDefaultLighting();
                    }
                    //Draw each mesh of the model
                    mesh.Draw();
                }
            }
        }
    }
}