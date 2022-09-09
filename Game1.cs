using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TankGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        public Texture2D heightMapTexture;
        public Texture2D tileTexture;
        Terrain terrain;
        Camera camera;
        Tank tank;
        EnemyTank enemyTank;
        ParticleSystem particles;
        Bullet bullet;
        EnemyBullet enemyBullet;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Tanks positions initialization
            Vector3 playerTankPosition = new Vector3(64.0f, 0.0f, 64.0f);
            Vector3 enemyTankPosition = new Vector3(50.0f, 0.0f, 50.0f);

            heightMapTexture = Content.Load<Texture2D>("lh3d1");
            tileTexture = Content.Load<Texture2D>("cobble");
            terrain = new Terrain(GraphicsDevice, heightMapTexture, tileTexture);
            tank = new Tank(GraphicsDevice, terrain, Content.Load<Model>("tank"), playerTankPosition);
            enemyTank = new EnemyTank(GraphicsDevice, terrain, Content.Load<Model>("tank"), enemyTankPosition);
            camera = new Camera(GraphicsDevice, tank.position, 0, 0, heightMapTexture, tileTexture, tank);
            particles = new ParticleSystem(GraphicsDevice, Content, tank, enemyTank, terrain, camera);
            bullet = new Bullet(GraphicsDevice, Content);
            enemyBullet = new EnemyBullet(GraphicsDevice, Content);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState ms = Mouse.GetState();
            KeyboardState kb = Keyboard.GetState();

            camera.Update(GraphicsDevice, gameTime, kb, ms);
            base.Update(gameTime);
            tank.Update(gameTime, kb, terrain, enemyTank);
            enemyTank.Update(gameTime, kb, terrain, tank);
            particles.Update(gameTime, kb);
            bullet.Update(gameTime, kb, terrain, tank, enemyTank);
            enemyBullet.Update(gameTime, kb, terrain, tank, enemyTank);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            terrain.Draw(GraphicsDevice, camera);
            tank.Draw(GraphicsDevice, camera);
            enemyTank.Draw(GraphicsDevice, camera);
            particles.Draw(GraphicsDevice);
            bullet.Draw(GraphicsDevice, camera);
            enemyBullet.Draw(GraphicsDevice, camera);

            base.Draw(gameTime);
        }
    }
}
