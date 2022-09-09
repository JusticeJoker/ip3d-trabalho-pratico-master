using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TankGame
{
    class ParticleSystem
    {
        List<Particle> particleLists;   // particulas
        Particle tempParticle;

        Random rand;

        Tank tank;
        EnemyTank enemyTank;
        Terrain terrain;
        Camera camera;

        float distance;

        GraphicsDevice device;
        ContentManager content;

        public ParticleSystem(GraphicsDevice device, ContentManager content, Tank tank, EnemyTank enemyTank, Terrain terrain, Camera camera)
        {
            this.device = device;
            this.terrain = terrain;
            this.content = content;
            this.camera = camera;
            this.tank = tank;

            this.enemyTank = enemyTank;
            rand = new Random();

            distance = 0.1f;
            particleLists = new List<Particle>();
        }

        public void CreateParticles(Vector3 Center)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 tempPos = new Vector3(Center.X + (distance * 2 * (float)rand.NextDouble() - distance), Center.Y - 0.2f, Center.Z + (distance * 2 * (float)rand.NextDouble() - distance));
                Vector3 tempVel = new Vector3(4 * (float)rand.NextDouble() - 2, 1.0f * 0.3f, 4 * (float)rand.NextDouble() - 2);
                tempParticle = new Particle(device, content, tempPos, tempVel);

                particleLists.Add(tempParticle);
            }
        }

        public void Update(GameTime gametime, KeyboardState kb)
        {
            if (kb.IsKeyDown(Keys.W))
            {
                CreateParticles(tank.boneTransforms[2].Translation);
                CreateParticles(tank.boneTransforms[6].Translation);
            }
            else if (kb.IsKeyDown(Keys.I))
            {
                CreateParticles(enemyTank.boneTransforms[2].Translation);
                CreateParticles(enemyTank.boneTransforms[6].Translation);
            }

            foreach (Particle p in particleLists.ToArray())
            {
                p.Update(gametime);

                if (p.position.Y < terrain.GetHeight(p.position.X, p.position.Z))
                {
                    particleLists.Remove(p);
                }
            }
        }

        public void Draw(GraphicsDevice device)
        {

            foreach (Particle p in particleLists)
            {
                p.Draw(device, camera);
            }

        }
    }
}

