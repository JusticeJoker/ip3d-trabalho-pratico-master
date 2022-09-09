using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TankGame
{
    class EnemyTank
    {
        Model myModel;
        float scale;

        public Vector3 position;
        public Vector3 direction;
        public Vector3 normal;

        float tankRotation;
        public float yaw;

        ModelBone turretBone;
        ModelBone cannonBone;

        public Matrix[] boneTransforms;

        Matrix rotation;

        float tankCollisionBoxRadius = 1.0f;
        Vector3 pos;
        Vector3 tankDistance;

        bool pursuit;
        float maxBoidRadius = 150;
        float minBoidRadius = 50;
        float maxAcl = 200.0f;
        float wonderingAcl = 150.0f;
        Random rand;
        float radius = 1f;
        Vector3 perfectDirection;
        Vector3 wanderingDirection;
        float speed = 30.0f;
        float maxSpeed = 20.0f;

        public EnemyTank(GraphicsDevice device, Terrain terrain, Model m, Vector3 initialPosition)
        {
            this.myModel = m;

            scale = 0.002f;
            position = initialPosition;
            position.Y = terrain.GetHeight(position.X, position.Z);
            direction = new Vector3(0.0f, 0.0f, 1.0f);
            tankRotation = 0.0f;
            rand = new Random();

            boneTransforms = new Matrix[myModel.Bones.Count];

            rotation = Matrix.Identity;
        }

        public void Update(GameTime gameTime, KeyboardState kb, Terrain terrain, Tank enemyTank)
        {
            pos = position;

            Matrix scale = Matrix.CreateScale(this.scale);
            Matrix translation = Matrix.CreateTranslation(position);

            myModel.Root.Transform = scale * rotation * translation;
            myModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Tank movement variables
            float rotationSpeed = 50.0f;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float rotationTransformationFactor = rotationSpeed * deltaTime;
            Vector3 baseVector = -Vector3.UnitZ;

            if (kb.IsKeyDown(Keys.D1))
                pursuit = true;
            if (kb.IsKeyDown(Keys.D2))
                pursuit = false;

            // Tank rotation
            if (kb.IsKeyDown(Keys.I) || kb.IsKeyDown(Keys.K))
            {
                if (kb.IsKeyDown(Keys.J))
                    tankRotation += rotationTransformationFactor;
                if (kb.IsKeyDown(Keys.L))
                    tankRotation -= rotationTransformationFactor;
            }
            
            yaw = MathHelper.ToRadians(tankRotation);
            rotation = Matrix.CreateRotationY(yaw);
            direction = Vector3.Transform(baseVector, Matrix.CreateRotationY(yaw));
            Vector3 positionTransformationFactor = direction * deltaTime;

            // Tank movement
            if (kb.IsKeyDown(Keys.I))
                pos += positionTransformationFactor;
            if (kb.IsKeyDown(Keys.K))
                pos -= positionTransformationFactor;


            // Collisions
            tankDistance = position - enemyTank.position;

            if (pos.X < terrain.terrainWidth - 1 && pos.X > 0 && pos.Z < terrain.terrainHeight - 1 && pos.Z > 0 && !(tankDistance.Length() < 2.0f * tankCollisionBoxRadius))
                position = pos;


            // Autonomous Movement

            // Wandering
            if (pursuit == true && tankDistance.Length() >= minBoidRadius && tankDistance.Length() <= maxBoidRadius)
            {
                float random = (float)rand.Next(0, 1);
                perfectDirection = enemyTank.pos - pos;
                Vector3 circlePosition = Vector3.UnitZ;

                if (pos.Z <= -64 && pos.X <= 64)
                {
                    circlePosition = new Vector3(pos.X + (radius * 2f), 0, pos.Z + (radius * 2f));
                }
                else if (pos.Z <= -64 && pos.X > 64)
                {
                    circlePosition = new Vector3(pos.X - (radius * 2f), 0, pos.Z + (radius * 2f));
                }
                else if (pos.Z > -64 && pos.X <= 64)
                {
                    circlePosition = new Vector3(pos.X + (radius * 2f), 0, pos.Z - (radius * 2f));
                }
                else if (pos.Z > -64 && pos.X > 64)
                {
                    circlePosition = new Vector3(pos.X - (radius * 2f), 0, pos.Z - (radius * 2f));
                }

                Vector3 randomCircunferencePoint = new Vector3(radius * (float)Math.Cos(random) + circlePosition.X, 0, radius * (float)Math.Sin(random) + circlePosition.Z);


                wanderingDirection = (randomCircunferencePoint - pos) * 0.7f + perfectDirection * 0.3f;
                wanderingDirection.Normalize();

                Vector3 tankSpeed = direction * speed;
                Vector3 wanderingSpeed = wanderingDirection * maxSpeed;
                wanderingSpeed.Normalize();

                Vector3 wanderingAcl = (wanderingSpeed - tankSpeed);
                wanderingAcl.Normalize();
                wanderingAcl = wanderingAcl * wonderingAcl;

                tankSpeed = tankSpeed + wanderingAcl * (float)gameTime.ElapsedGameTime.TotalSeconds;
                pos = pos + tankSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                direction = tankSpeed;
                direction.Normalize();

                speed = tankSpeed.Length();

                if (pos.X < terrain.terrainWidth - 1 && pos.X > 0 && pos.Z < terrain.terrainHeight - 1 && pos.Z > 0 && !(tankDistance.Length() < tankCollisionBoxRadius))
                    position = pos;
            }

            // Pursuit
            if (pursuit == true && tankDistance.Length() <= minBoidRadius)
            {
                perfectDirection = enemyTank.pos - pos;
                perfectDirection.Normalize();
                Vector3 directionSeek = perfectDirection;

                Vector3 tankSpeed = direction * speed;
                Vector3 velocitySeek = directionSeek * maxSpeed;

                Vector3 acceleration = (velocitySeek - tankSpeed);
                acceleration.Normalize();
                acceleration = acceleration * maxAcl;

                tankSpeed = tankSpeed + acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;

                pos = pos + tankSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                direction = tankSpeed;
                direction.Normalize();

                speed = tankSpeed.Length();

                if (pos.X < terrain.terrainWidth - 1 && pos.X > 0 && pos.Z < terrain.terrainHeight - 1 && pos.Z > 0 && !(tankDistance.Length() < tankCollisionBoxRadius))
                    position = pos;
            }

            position.Y = terrain.GetHeight(position.X, position.Z);
            rotation.Up = terrain.GetNormal(position.X, position.Z);
            rotation.Right = Vector3.Cross(rotation.Up, direction);
            rotation.Forward = Vector3.Cross(rotation.Up, rotation.Right);
            normal = terrain.GetNormal(position.X, position.Z);
        }

        public void Draw(GraphicsDevice device, Camera camara)
        {
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index];
                    effect.View = camara.viewMatrix;
                    effect.Projection = camara.projectionMatrix;
                    effect.EnableDefaultLighting();
                }

                // Draw each mesh of the model
                mesh.Draw();
            }
        }
    }
}
