using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankGame
{
    class Tank
    {
        Model myModel;
        float scale;

        public Vector3 position;
        public Vector3 direction;
        public Vector3 normal;
        float speed = 2.0f;

        float tankRotation;
        public float yaw;
        // Turret and cannon bones
        ModelBone turretBone;
        ModelBone cannonBone;

        // Default transforms
        Matrix cannonTransform;
        Matrix turretTransform;

        // Keeps all transforms
        public Matrix[] boneTransforms;

        float turrentAngle = 0.0f;
        float cannonAngle = 0.0f;

        Matrix rotation;

        float tankCollisionBoxRadius = 1.0f;
        public Vector3 pos;

        public Tank(GraphicsDevice device, Terrain terrain, Model m, Vector3 initialPosition)
        {
            this.myModel = m;

            scale = 0.002f;
            position = initialPosition;
            position.Y = terrain.GetHeight(position.X, position.Z);
            direction = new Vector3(0.0f, 0.0f, 1.0f);
            tankRotation = 0.0f;

            turretBone = myModel.Bones["turret_geo"];
            cannonBone = myModel.Bones["canon_geo"];

            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;

            boneTransforms = new Matrix[myModel.Bones.Count];

            rotation = Matrix.Identity;
        }

        public void Update(GameTime gameTime, KeyboardState kb, Terrain terrain, EnemyTank enemyTank)
        {
            pos = position;

            Matrix scale = Matrix.CreateScale(this.scale);
            Matrix translation = Matrix.CreateTranslation(position);

            myModel.Root.Transform = scale * rotation * translation;
            myModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            // Turret rotation
            if (kb.IsKeyDown(Keys.Left))
                turrentAngle += MathHelper.ToRadians(1.0f);
            if (kb.IsKeyDown(Keys.Right))
                turrentAngle -= MathHelper.ToRadians(1.0f);

            // Cannon elevation
            if (kb.IsKeyDown(Keys.Down))
                cannonAngle += MathHelper.ToRadians(1.0f);
            if (kb.IsKeyDown(Keys.Up))
                cannonAngle -= MathHelper.ToRadians(1.0f);

            turretBone.Transform = Matrix.CreateRotationY(turrentAngle) * turretTransform;
            cannonBone.Transform = Matrix.CreateRotationX(cannonAngle) * cannonTransform;

            // Tank movement variablesd
            float rotationSpeed = 50.0f;
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float rotationTransformationFactor = rotationSpeed * deltaTime;
            Vector3 baseVector = -Vector3.UnitZ;

            // Tank rotation
            if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.S))
            {
                if (kb.IsKeyDown(Keys.A))
                    tankRotation += rotationTransformationFactor;
                if (kb.IsKeyDown(Keys.D))
                    tankRotation -= rotationTransformationFactor;
            }

            yaw = MathHelper.ToRadians(tankRotation);
            rotation = Matrix.CreateRotationY(yaw);
            direction = Vector3.Transform(baseVector, Matrix.CreateRotationY(yaw));
            Vector3 positionTransformationFactor = direction * speed * deltaTime;

            // Tank movement
            if (kb.IsKeyDown(Keys.W))
                pos += positionTransformationFactor;
            if (kb.IsKeyDown(Keys.S))
                pos -= positionTransformationFactor;


            // Collisions
            Vector3 tankDistance;
            tankDistance = position - enemyTank.position;

            if (pos.X < terrain.terrainWidth - 1 && pos.X > 0 && pos.Z < terrain.terrainHeight - 1 && pos.Z > 0 && !(tankDistance.Length() < tankCollisionBoxRadius))
                position = pos;

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
