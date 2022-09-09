 using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TankGame
{
    class Terrain
    {
        float[] heightMap;
        short[] indices;
        public int terrainWidth, terrainHeight;
        int indexCount;
        int vertexCount;
        VertexBuffer vBuffer;
        IndexBuffer iBuffer;
        BasicEffect effect;
        Matrix world;
        public Vector3[] normalMap;
        VertexPositionNormalTexture[] vertices;

        public Terrain(GraphicsDevice device, Texture2D heightMapTexture, Texture2D tileTexture)
        {
            effect = new BasicEffect(device);

            float aspectRatio = (float)device.Viewport.Width /
                           device.Viewport.Height;
            effect.View = Matrix.CreateLookAt(
                                new Vector3(64.0f, 5.0f, 64.0f),
                                new Vector3(64.0f, 0.0f, 0.0f),
                               Vector3.Up);
            effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                                MathHelper.ToRadians(45.0f),
                                aspectRatio, 0.1f, 1000.0f);
            effect.LightingEnabled = false;
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;
            effect.Texture = tileTexture;

            // Cria os eixos 3D
            CreateGeometry(device, heightMapTexture);
        }

        public void CreateGeometry(GraphicsDevice device, Texture2D heightMapTexture)
        {
            float verticalScale = 0.04f; // Defines height scale. Otherwise somes points would have up to 250 meters
            int textureSize = heightMapTexture.Width * heightMapTexture.Height;
            Color[] heightColors;
            heightColors = new Color[textureSize]; // Array Size = Texture Size
            heightMapTexture.GetData<Color>(heightColors); // Picks info from the heightMap and stores the data on the heightColors array

            heightMap = new float[heightMapTexture.Width * heightMapTexture.Height];

            // Using each pixels color to calculate corresponding height
            for (int i = 0; i < textureSize; i++)
                heightMap[i] = heightColors[i].R * verticalScale;

            // Terrain Size = Texture Size
            terrainWidth = heightMapTexture.Width;
            terrainHeight = heightMapTexture.Height;

            vertexCount = terrainWidth * terrainHeight;
            vertices = new VertexPositionNormalTexture[vertexCount];

            // Vertices calculation
            for (int z = 0; z < terrainHeight; z++)
            {
                for (int x = 0; x < terrainWidth; x++)
                {
                    int i;
                    i = z * terrainWidth + x;

                    float h, u, v; // h = Height; u & v are variables for texture coordinates
                    h = heightMap[i];
                    u = x % 2;
                    v = z % 2;

                    vertices[i] = new VertexPositionNormalTexture(
                        new Vector3(x, h, z),
                        Vector3.Up,
                        new Vector2(u, v));
                }
            }

            ComputeNormals();

            vBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), vertexCount, BufferUsage.None);
            vBuffer.SetData<VertexPositionNormalTexture>(vertices);

            indexCount = (terrainWidth - 1) * 2 * terrainHeight; // 2 indices in each side of the strip * terrain height (vertically)

            indices = new short[indexCount];

            // Indices attribution
            for (int strip = 0; strip < terrainWidth - 1; strip++)
            {
                for (int linha = 0; linha < terrainHeight; linha++)
                {
                    indices[strip * (2 * terrainHeight) + 2 * linha + 0] = (short)(strip + linha * terrainWidth + 0);
                    indices[strip * (2 * terrainHeight) + 2 * linha + 1] = (short)(strip + linha * terrainWidth + 1);
                }
            }

            iBuffer = new IndexBuffer(device, typeof(short), indexCount, BufferUsage.None);
            iBuffer.SetData<short>(indices);
        }

        public void Update(GameTime gameTime)
        {
            world = Matrix.Identity;
        }

        public void Draw(GraphicsDevice device, Camera camera)
        {
            effect.Projection = camera.projectionMatrix;
            effect.View = camera.viewMatrix;
            effect.World = Matrix.Identity;            

            effect.CurrentTechnique.Passes[0].Apply();
            device.SetVertexBuffer(vBuffer);
            device.Indices = iBuffer;

            for (int strip = 0; strip < terrainWidth - 1; strip++)
            {
                device.DrawIndexedPrimitives(
                    PrimitiveType.TriangleStrip,
                    0,
                    strip * (2 * terrainHeight),
                    terrainHeight * 2 - 2
                    );
            }
        }

        public float GetHeight(float positionX, float positionZ)
        {
            int xa = (int)positionX;
            int za = (int)positionZ;
            int ia = za * terrainWidth + xa;
            float ya = heightMap[ia];

            int xb = (int)positionX + 1;
            int zb = (int)positionZ;
            int ib = zb * terrainWidth + xb;
            float yb = heightMap[ib];

            int xc = (int)positionX;
            int zc = (int)positionZ + 1;
            int ic = zc * terrainWidth + xc;
            float yc = heightMap[ic];

            int xd = (int)positionX + 1;
            int zd = (int)positionZ + 1;
            int id = zd * terrainWidth + xd;
            float yd = heightMap[id];

            float da = positionX - xa;
            float db = xb - positionX;
            float yab = ya * db + yb * da;

            float dc = positionX - xc;
            float dd = xd - positionX;
            float ycd = yc * dd + yd * dc;

            float dab = positionZ - za;
            float dcd = zc - positionZ;

            float y = yab * dcd + ycd * dab;

            return y;
        }

        public void ComputeNormals()
        {
            normalMap = new Vector3[terrainHeight * terrainWidth];

            // Variables
            Vector3 averageNormal;
            Vector3 vE, vE1, vE2, vE3, vE1_2, vE2_3;

            // Uper-Left Corner
            {
                vE = vertices[0].Position;
                vE1 = vertices[1 * terrainWidth].Position - vE;
                vE2 = vertices[1 * terrainWidth + 1].Position - vE;
                vE3 = vertices[0 * terrainWidth + 1].Position - vE;

                vE1.Normalize();
                vE2.Normalize();
                vE3.Normalize();

                vE1_2 = Vector3.Cross(vE1, vE2);
                vE2_3 = Vector3.Cross(vE2, vE3);

                vE1_2.Normalize();
                vE2_3.Normalize();

                averageNormal = (vE1_2 + vE2_3) / 2;

                normalMap[0] = averageNormal;
                vertices[0].Normal = averageNormal;
            }

            // Uper-Right Corner
            {
                vE = vertices[terrainWidth - 1].Position;
                vE1 = vertices[terrainWidth - 2].Position - vE;
                vE2 = vertices[2 * terrainWidth - 2].Position - vE;
                vE3 = vertices[2 * terrainWidth - 1].Position - vE;

                vE1.Normalize();
                vE2.Normalize();
                vE3.Normalize();

                vE1_2 = Vector3.Cross(vE1, vE2);
                vE2_3 = Vector3.Cross(vE2, vE3);

                vE1_2.Normalize();
                vE2_3.Normalize();

                averageNormal = (vE1_2 + vE2_3) / 2;

                normalMap[terrainWidth - 1] = averageNormal;
                vertices[terrainWidth - 1].Normal = averageNormal;
            }

            // Bottom-Left Corner
            {
                vE = vertices[(terrainHeight - 1) * terrainWidth].Position;
                vE1 = vertices[(terrainHeight - 1) * terrainWidth + 1].Position - vE;
                vE2 = vertices[(terrainHeight - 2) * terrainWidth + 1].Position - vE;
                vE3 = vertices[(terrainHeight - 2) * terrainWidth].Position - vE;

                vE1.Normalize();
                vE2.Normalize();
                vE3.Normalize();

                vE1_2 = Vector3.Cross(vE1, vE2);
                vE2_3 = Vector3.Cross(vE2, vE3);

                vE1_2.Normalize();
                vE2_3.Normalize();

                averageNormal = (vE1_2 + vE2_3) / 2;

                normalMap[(terrainHeight - 1) * terrainWidth] = averageNormal;
                vertices[(terrainHeight - 1) * terrainWidth].Normal = averageNormal;
            }

            // Bottom-Right Corner
            {
                vE = vertices[(terrainHeight - 1) + (terrainWidth - 1)].Position;
                vE1 = vertices[(terrainHeight - 2) + (terrainWidth - 1)].Position - vE;
                vE2 = vertices[(terrainHeight - 2) + (terrainWidth - 2)].Position - vE;
                vE3 = vertices[(terrainHeight - 1) + (terrainWidth - 2)].Position - vE;

                vE1.Normalize();
                vE2.Normalize();
                vE3.Normalize();

                vE1_2 = Vector3.Cross(vE1, vE2);
                vE2_3 = Vector3.Cross(vE2, vE3);

                vE1_2.Normalize();
                vE2_3.Normalize();

                averageNormal = (vE1_2 + vE2_3) / 2;

                normalMap[(terrainHeight - 1) + (terrainWidth - 1)] = averageNormal;
                vertices[(terrainHeight - 1) + (terrainWidth - 1)].Normal = averageNormal;
            }

            // Left Side
            {
                for (int z = 1; z < terrainHeight - 1; z++)
                {
                    Vector3 v = vertices[z * terrainWidth].Position;
                    Vector3 v1 = vertices[(z + 1) * terrainWidth].Position - v;
                    Vector3 v2 = vertices[(z + 1) * terrainWidth + 1].Position - v;
                    Vector3 v3 = vertices[z * terrainWidth + 1].Position - v;
                    Vector3 v4 = vertices[(z - 1) * terrainWidth + 1].Position - v;
                    Vector3 v5 = vertices[(z - 1) * terrainWidth].Position - v;

                    v1.Normalize();
                    v2.Normalize();
                    v3.Normalize();
                    v4.Normalize();
                    v5.Normalize();

                    Vector3 n1_2 = Vector3.Cross(v1, v2);
                    Vector3 n2_3 = Vector3.Cross(v2, v3);
                    Vector3 n3_4 = Vector3.Cross(v3, v4);
                    Vector3 n4_5 = Vector3.Cross(v4, v5);

                    n1_2.Normalize();
                    n2_3.Normalize();
                    n3_4.Normalize();
                    n4_5.Normalize();

                    averageNormal = (n1_2 + n2_3 + n3_4 + n4_5) / 4;

                    normalMap[z * terrainWidth] = averageNormal;
                    vertices[z * terrainWidth].Normal = averageNormal;
                }
            }

            // Right Side
            {
                for (int z = 1; z < terrainHeight - 1; z++)
                {
                    Vector3 v = vertices[z * terrainWidth + (terrainWidth - 1)].Position;
                    Vector3 v1 = vertices[(z - 1) * terrainWidth + (terrainWidth - 1)].Position - v;
                    Vector3 v2 = vertices[(z - 1) * terrainWidth + (terrainWidth - 2)].Position - v;
                    Vector3 v3 = vertices[z * terrainWidth + (terrainWidth - 2)].Position - v;
                    Vector3 v4 = vertices[(z + 1) * terrainWidth + (terrainWidth - 2)].Position - v;
                    Vector3 v5 = vertices[(z + 1) * terrainWidth + (terrainWidth - 1)].Position - v;

                    v1.Normalize();
                    v2.Normalize();
                    v3.Normalize();
                    v4.Normalize();
                    v5.Normalize();

                    Vector3 n1_2 = Vector3.Cross(v1, v2);
                    Vector3 n2_3 = Vector3.Cross(v2, v3);
                    Vector3 n3_4 = Vector3.Cross(v3, v4);
                    Vector3 n4_5 = Vector3.Cross(v4, v5);

                    n1_2.Normalize();
                    n2_3.Normalize();
                    n3_4.Normalize();
                    n4_5.Normalize();

                    averageNormal = (n1_2 + n2_3 + n3_4 + n4_5) / 4;

                    normalMap[z * terrainWidth + (terrainWidth - 1)] = averageNormal;
                    vertices[z * terrainWidth + (terrainWidth - 1)].Normal = averageNormal;
                }
            }

            // Upper Side
            {
                for (int x = 1; x < terrainWidth - 1; x++)
                {
                    Vector3 v = vertices[0 * terrainWidth + x].Position;
                    Vector3 v1 = vertices[0 * terrainWidth + (x - 1)].Position - v;
                    Vector3 v2 = vertices[1 * terrainWidth + (x - 1)].Position - v;
                    Vector3 v3 = vertices[1 * terrainWidth + x].Position - v;
                    Vector3 v4 = vertices[1 * terrainWidth + (x + 1)].Position - v;
                    Vector3 v5 = vertices[0 * terrainWidth + (x + 1)].Position - v;

                    v1.Normalize();
                    v2.Normalize();
                    v3.Normalize();
                    v4.Normalize();
                    v5.Normalize();

                    Vector3 n1_2 = Vector3.Cross(v1, v2);
                    Vector3 n2_3 = Vector3.Cross(v2, v3);
                    Vector3 n3_4 = Vector3.Cross(v3, v4);
                    Vector3 n4_5 = Vector3.Cross(v4, v5);

                    n1_2.Normalize();
                    n2_3.Normalize();
                    n3_4.Normalize();
                    n4_5.Normalize();

                    averageNormal = (n1_2 + n2_3 + n3_4 + n4_5) / 4;

                    normalMap[0 * terrainWidth + x] = averageNormal;
                    vertices[0 * terrainWidth + x].Normal = averageNormal;
                }
            }

            // Bottom Side
            {
                for (int x = 1; x < terrainWidth - 1; x++)
                {
                    Vector3 v = vertices[(terrainHeight - 1) * terrainWidth + x].Position;
                    Vector3 v1 = vertices[(terrainHeight - 1) * terrainWidth + (x + 1)].Position - v;
                    Vector3 v2 = vertices[(terrainHeight - 2) * terrainWidth + (x + 1)].Position - v;
                    Vector3 v3 = vertices[(terrainHeight - 2) * terrainWidth + x].Position - v;
                    Vector3 v4 = vertices[(terrainHeight - 2) * terrainWidth + (x - 1)].Position - v;
                    Vector3 v5 = vertices[(terrainHeight - 1) * terrainWidth + (x - 1)].Position - v;

                    v1.Normalize();
                    v2.Normalize();
                    v3.Normalize();
                    v4.Normalize();
                    v5.Normalize();

                    Vector3 n1_2 = Vector3.Cross(v1, v2);
                    Vector3 n2_3 = Vector3.Cross(v2, v3);
                    Vector3 n3_4 = Vector3.Cross(v3, v4);
                    Vector3 n4_5 = Vector3.Cross(v4, v5);

                    n1_2.Normalize();
                    n2_3.Normalize();
                    n3_4.Normalize();
                    n4_5.Normalize();

                    averageNormal = (n1_2 + n2_3 + n3_4 + n4_5) / 4;

                    normalMap[(terrainHeight - 1) * terrainWidth + x] = averageNormal;
                    vertices[(terrainHeight - 1) * terrainWidth + x].Normal = averageNormal;
                }
            }

            // Middle Cases
            {
                for (int z = 1; z < terrainHeight - 1; z++)
                {
                    for (int x = 1; x < terrainWidth - 1; x++)
                    {
                        Vector3 v = vertices[z * terrainWidth + x].Position;
                        Vector3 v1 = vertices[(z - 1) * terrainWidth + x].Position - v;
                        Vector3 v2 = vertices[(z - 1) * terrainWidth + (x - 1)].Position - v;
                        Vector3 v3 = vertices[z * terrainWidth + (x - 1)].Position - v;
                        Vector3 v4 = vertices[(z + 1) * terrainWidth + (x - 1)].Position - v;
                        Vector3 v5 = vertices[(z + 1) * terrainWidth + x].Position - v;
                        Vector3 v6 = vertices[(z + 1) * terrainWidth + (x + 1)].Position - v;
                        Vector3 v7 = vertices[z * terrainWidth + (x + 1)].Position - v;
                        Vector3 v8 = vertices[(z - 1) * terrainWidth + (x + 1)].Position - v;

                        v1.Normalize();
                        v2.Normalize();
                        v3.Normalize();
                        v4.Normalize();
                        v5.Normalize();
                        v6.Normalize();
                        v7.Normalize();
                        v8.Normalize();

                        Vector3 n1_2 = Vector3.Cross(v1, v2);
                        Vector3 n2_3 = Vector3.Cross(v2, v3);
                        Vector3 n3_4 = Vector3.Cross(v3, v4);
                        Vector3 n4_5 = Vector3.Cross(v4, v5);
                        Vector3 n5_6 = Vector3.Cross(v5, v6);
                        Vector3 n6_7 = Vector3.Cross(v6, v7);
                        Vector3 n7_8 = Vector3.Cross(v7, v8);
                        Vector3 n8_1 = Vector3.Cross(v8, v1);

                        n1_2.Normalize();
                        n2_3.Normalize();
                        n3_4.Normalize();
                        n4_5.Normalize();
                        n5_6.Normalize();
                        n6_7.Normalize();
                        n7_8.Normalize();
                        n8_1.Normalize();

                        averageNormal = (n1_2 + n2_3 + n3_4 + n4_5 + n5_6 + n6_7 + n7_8 + n7_8 + n8_1) / 8;

                        normalMap[z * terrainWidth + x] = averageNormal;
                        vertices[z * terrainWidth + x].Normal = averageNormal;
                    }
                }
            }
        }

        public Vector3 GetNormal(float positionX, float positionZ)
        {
            int xa = (int)positionX;
            int za = (int)positionZ;
            int ia = za * terrainWidth + xa;
            Vector3 ya = normalMap[ia];

            int xb = (int)positionX + 1;
            int zb = (int)positionZ;
            int ib = zb * terrainWidth + xb;
            Vector3 yb = normalMap[ib];

            int xc = (int)positionX;
            int zc = (int)positionZ + 1;
            int ic = zc * terrainWidth + xc;
            Vector3 yc = normalMap[ic];

            int xd = (int)positionX + 1;
            int zd = (int)positionZ + 1;
            int id = zd * terrainWidth + xd;
            Vector3 yd = normalMap[id];

            float da = positionX - xa;
            float db = xb - positionX;
            Vector3 yab = ya * db + yb * da;

            float dc = positionX - xc;
            float dd = xd - positionX;
            Vector3 ycd = yc * dd + yd * dc;

            float dab = positionZ - za;
            float dcd = zc - positionZ;

            Vector3 y = yab * dcd + ycd * dab;

            return y;
        }
    }
}
