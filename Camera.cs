using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankGame
{
    class Camera
    {
        public Matrix viewMatrix;
        public Matrix projectionMatrix;
        float yaw, pitch;
        Vector3 position;
        int viewportWidth, viewportHeight;
        float speed;
        float offset;
        Terrain terrain;
        int camType = 2; /* Defines wich camera is in use
                            0 - Surface Follow Camera
                            1 - Free Camera
                            2 - Third Person Camera */

        Tank tank; // Used to update the third person camera

        public Camera(GraphicsDevice device, Vector3 position, float yaw, float pitch, Texture2D heightMapTexture, Texture2D tileTexture, Tank tank)
        {
            this.yaw = yaw;
            this.pitch = pitch;
            this.position = position;
            this.viewportWidth = device.Viewport.Width;
            this.viewportHeight = device.Viewport.Height;
            speed = 10.0f;
            terrain = new Terrain(device, heightMapTexture, tileTexture);
            offset = 1.5f;
            this.tank = tank;

            Matrix rotation = Matrix.CreateFromYawPitchRoll(this.yaw, this.pitch, 0.0f);
            Vector3 direcao = Vector3.Transform(Vector3.UnitX, rotation);
            Vector3 target = this.position + direcao;

            // Compute aspect ratio, the view matrix and the projection matrix
            float aspectRatio = (float)device.Viewport.Width  /device.Viewport.Height;
            viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 0.1f, 1000.0f);
        }

        public void Update(GraphicsDevice device, GameTime gameTime, KeyboardState keyboardState, MouseState mouseState)
        {
            // Change cameras
            if (keyboardState.IsKeyDown(Keys.F1))
                camType = 0;
            else if (keyboardState.IsKeyDown(Keys.F2))
                camType = 1;
            else if (keyboardState.IsKeyDown(Keys.F3))
                camType = 2;

            // Surface Follow and Free Camera Control
            if (camType == 0 || camType == 1)
            {
                float degreesPerPixelX = 0.3f;
                float degreesPerPixelY = 0.3f;

                // Comparing current mouse position versus previous mouse position
                int deltaX = mouseState.Position.X - viewportWidth / 2;
                int deltaY = mouseState.Position.Y - viewportHeight / 2;

                // Negative = -yaw; Positive = +yaw
                yaw -= deltaX * MathHelper.ToRadians(degreesPerPixelX);
                pitch -= deltaY * MathHelper.ToRadians(degreesPerPixelY);

                Vector3 baseVector = Vector3.UnitZ;
                Vector3 directionH = Vector3.Transform(baseVector, Matrix.CreateRotationY(yaw));
                Vector3 right = Vector3.Cross(directionH, Vector3.UnitY);
                Matrix rotationPitch = Matrix.CreateFromAxisAngle(right, pitch);
                Vector3 direction = Vector3.Transform(directionH, rotationPitch);

                Vector3 previousPos;
                previousPos = position;

                // Camera control keys
                if (keyboardState.IsKeyDown(Keys.NumPad8))
                    position += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (keyboardState.IsKeyDown(Keys.NumPad5))
                    position -= direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (keyboardState.IsKeyDown(Keys.NumPad6))
                    position += right * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (keyboardState.IsKeyDown(Keys.NumPad4))
                    position -= right * (speed / 2.0f) * (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (camType == 0)
                    position.Y = terrain.GetHeight(position.X, position.Z) + offset;

                if (camType == 1)
                {
                    if (keyboardState.IsKeyDown(Keys.NumPad1))
                        position.Y -= speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (keyboardState.IsKeyDown(Keys.NumPad7))
                        position.Y += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                Vector3 target = position + direction;

                viewMatrix = Matrix.CreateLookAt(
                    this.position,
                    target,
                    Vector3.Up
                    );

                Mouse.SetPosition(viewportWidth / 2, viewportHeight / 2);
            }

            else if (camType == 2)
            {
                position = tank.position + tank.direction * -6 + tank.normal * 1;
                position.Y = terrain.GetHeight(position.X, position.Z) + 0.7f;

                int centerX = device.Viewport.Width / 2;
                int centerY = device.Viewport.Height / 2;

                Matrix rotation = Matrix.CreateFromYawPitchRoll(yaw, pitch, 0.0f);
                Vector3 direction = Vector3.Transform(-Vector3.UnitZ, rotation);
                Vector3 right = Vector3.Cross(direction, Vector3.Up);

                Vector3 previousPos;
                previousPos = position;

                // Camera position relative to the tank
                position = tank.position + tank.direction * 6 + tank.normal * 1;
                direction = tank.direction;

                if (position.X >= 126 || position.X <= 1 || position.Z <= -126 || position.Z >= -1)
                {
                    position = previousPos;
                    yaw = tank.yaw;
                }

                Vector3 target = position + direction;

                this.viewMatrix = Matrix.CreateLookAt(position, target, Vector3.Up);
                Mouse.SetPosition(centerX, centerY);
            }
        }
    }
}

