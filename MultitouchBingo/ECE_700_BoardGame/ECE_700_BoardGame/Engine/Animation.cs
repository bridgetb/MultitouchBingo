//Needs Adapting and possibly inheriting from

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Media;

//namespace ECE_700_BoardGame.Engine
//{
//    /// <summary>
//    /// This is the main type for your game
//    /// </summary>
//    public class Game1 : Microsoft.Xna.Framework.Game
//    {
//        GraphicsDeviceManager graphics;
//        SpriteBatch spriteBatch;
//        // Represents the player
//        Player player;

//        // Keyboard states used to determine key presses
//        KeyboardState currentKeyboardState;
//        KeyboardState previousKeyboardState;

//        // Gamepad states used to determine button presses
//        GamePadState currentGamePadState;
//        GamePadState previousGamePadState;

//        // A movement speed for the player
//        float playerMoveSpeed;

//        // Image used to display the static background
//        Texture2D mainBackground;

//        // Parallaxing Layers
//        parallaxingBackground bgLayer1;
//        parallaxingBackground bgLayer2;

//        // Enemies
//        Texture2D enemyTexture;
//        List<enemy> enemies;

//        // The rate at which the enemies appear
//        TimeSpan enemySpawnTime;
//        TimeSpan previousSpawnTime;

//        // A random number generator
//        Random random;

//        Texture2D projectileTexture;
//        List<Projectile> projectiles;

//        // The rate of fire of the player laser
//        TimeSpan fireTime;
//        TimeSpan previousFireTime;

//        Texture2D explosionTexture;
//        List<Animation> explosions;

//        // The sound that is played when a laser is fired
//        SoundEffect laserSound;

//        // The sound used when the player or an enemy dies
//        SoundEffect explosionSound;

//        // The music played during gameplay
//        Song gameplayMusic;

//        // Number that holds the player score
//        int score;

//        // The font used to display UI elements
//        SpriteFont font;

//        public Game1()
//        {
//            graphics = new GraphicsDeviceManager(this);
//            Content.RootDirectory = "Content";
//        }

//        /// <summary>
//        /// Allows the game to perform any initialization it needs to before starting to run.
//        /// This is where it can query for any required services and load any non-graphic
//        /// related content.  Calling base.Initialize will enumerate through any components
//        /// and initialize them as well.
//        /// </summary>
//        protected override void Initialize()
//        {
//            // Initialize the player class
//            player = new Player();

//            // Set a constant player move speed
//            playerMoveSpeed = 8.0f;

//            // Init background
//            bgLayer1 = new parallaxingBackground();
//            bgLayer2 = new parallaxingBackground();

//            // Enable the FreeDrag gesture
//            TouchPanel.EnabledGestures = GestureType.FreeDrag;

//            // Initialize the enemies list
//            enemies = new List<enemy>();

//            // Set the time keepers to zero
//            previousSpawnTime = TimeSpan.Zero;

//            // Used to determine how fast enemy respawns
//            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

//            // Initialize our random number generator
//            random = new Random();

//            projectiles = new List<Projectile>();

//            // Set the laser to fire every quarter second
//            fireTime = TimeSpan.FromSeconds(.15f);

//            explosions = new List<Animation>();

//            // Set player's score to zero
//            score = 0;

//            base.Initialize();
//        }

//        /// <summary>
//        /// LoadContent will be called once per game and is the place to load
//        /// all of your content.
//        /// </summary>
//        protected override void LoadContent()
//        {
//            // Create a new SpriteBatch, which can be used to draw textures.
//            spriteBatch = new SpriteBatch(GraphicsDevice);

//            // Load the player resources
//            Animation playerAnimation = new Animation();
//            Texture2D playerTexture = Content.Load<Texture2D>("shipAnimation");
//            playerAnimation.Initialize(playerTexture, Vector2.Zero, 115, 69, 8, 30, Color.White, 1f, true);

//            Vector2 playerPosition = new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X, GraphicsDevice.Viewport.TitleSafeArea.Y +
//                                                    GraphicsDevice.Viewport.TitleSafeArea.Height / 2);
//            player.Initialize(playerAnimation, playerPosition);
//            //player.Initialize(Content.Load<Texture2D>("player"), playerPosition);

//            // Load the parallaxing background
//            bgLayer1.Initialize(Content, "bgLayer1", GraphicsDevice.Viewport.Width, -1);
//            bgLayer2.Initialize(Content, "bgLayer2", GraphicsDevice.Viewport.Width, -2);

//            mainBackground = Content.Load<Texture2D>("mainBackground");

//            // Enemy Texture resources
//            enemyTexture = Content.Load<Texture2D>("mineAnimation");

//            projectileTexture = Content.Load<Texture2D>("laser");

//            explosionTexture = Content.Load<Texture2D>("explosion");

//            // Load the music
//            gameplayMusic = Content.Load<Song>("sound/gameMusic");

//            SoundEffect.MasterVolume = 0.8f;

//            // Load the laser and explosion sound effect
//            laserSound = Content.Load<SoundEffect>("sound/laserFire");
//            explosionSound = Content.Load<SoundEffect>("sound/explosion");

//            // Load the score font
//            font = Content.Load<SpriteFont>("gameFont");

//            //Start the music right away
//            PlayMusic(gameplayMusic);
//        }

//        /// <summary>
//        /// UnloadContent will be called once per game and is the place to unload
//        /// all content.
//        /// </summary>
//        protected override void UnloadContent()
//        {
//            // TODO: Unload any non ContentManager content here
//        }

//        /// <summary>
//        /// Allows the game to run logic such as updating the world,
//        /// checking for collisions, gathering input, and playing audio.
//        /// </summary>
//        /// <param name="gameTime">Provides a snapshot of timing values.</param>
//        protected override void Update(GameTime gameTime)
//        {
//            // Allows the game to exit
//            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
//                this.Exit();

//            // Save the previous state of the keyboard and game pad so we can determine single key/button presses
//            previousGamePadState = currentGamePadState;
//            previousKeyboardState = currentKeyboardState;

//            // Read the current state of the keyboard and gamepad and store it
//            currentKeyboardState = Keyboard.GetState();
//            currentGamePadState = GamePad.GetState(PlayerIndex.One);

//            // Update the player
//            UpdatePlayer(gameTime);

//            bgLayer1.Update();
//            bgLayer2.Update();

//            // Update the enemies
//            UpdateEnemies(gameTime);

//            // Update the collision
//            UpdateCollision();

//            // Update the Projectiles
//            UpdateProjectiles();

//            // Update the explosions
//            UpdateExplosions(gameTime);

//            base.Update(gameTime);
//        }

//        /// <summary>
//        /// This updates the player's position
//        /// </summary>
//        private void UpdatePlayer(GameTime gameTime)
//        {
//            player.Update(gameTime);

//            // Windows Phone Controls
//            while (TouchPanel.IsGestureAvailable)
//            {
//                GestureSample gesture = TouchPanel.ReadGesture();
//                if (gesture.GestureType == GestureType.FreeDrag)
//                {
//                    player.Position += gesture.Delta;
//                }
//            }

//            // Get Thumbstick Controls
//            player.Position.X += currentGamePadState.ThumbSticks.Left.X * playerMoveSpeed;
//            player.Position.Y -= currentGamePadState.ThumbSticks.Left.Y * playerMoveSpeed;

//            // Use the Keyboard / Dpad
//            if (currentKeyboardState.IsKeyDown(Keys.Left) || currentGamePadState.DPad.Left == ButtonState.Pressed)
//            {
//                player.Position.X -= playerMoveSpeed;
//            }
//            if (currentKeyboardState.IsKeyDown(Keys.Right) || currentGamePadState.DPad.Right == ButtonState.Pressed)
//            {
//                player.Position.X += playerMoveSpeed;
//            }
//            if (currentKeyboardState.IsKeyDown(Keys.Up) || currentGamePadState.DPad.Up == ButtonState.Pressed)
//            {
//                player.Position.Y -= playerMoveSpeed;
//            }
//            if (currentKeyboardState.IsKeyDown(Keys.Down) || currentGamePadState.DPad.Down == ButtonState.Pressed)
//            {
//                player.Position.Y += playerMoveSpeed;
//            }

//            // Make sure that the player does not go out of bounds
//            player.Position.X = MathHelper.Clamp(player.Position.X, 0, GraphicsDevice.Viewport.Width - player.Width);
//            player.Position.Y = MathHelper.Clamp(player.Position.Y, 0, GraphicsDevice.Viewport.Height - player.Height);

//            // Fire only every interval we set as the fireTime
//            if (gameTime.TotalGameTime - previousFireTime > fireTime)
//            {
//                // Reset our current time
//                previousFireTime = gameTime.TotalGameTime;

//                // Add the projectile, but add it to the front and center of the player
//                AddProjectile(player.Position + new Vector2(player.Width / 2, 0));

//                // Play the laser sound
//                laserSound.Play();
//            }

//            // Reset score if payer health goes to zero
//            if (player.Health <= 0)
//            {
//                player.Health = 100;
//                score = 0;
//            }
//        }

//        /// <summary>
//        /// This is where the enemies are created
//        /// </summary>
//        /// 
//        private void AddEnemy()
//        {
//            // Create the animation object
//            Animation enemyAnimation = new Animation();

//            // Initialize the animation with the correct animation information
//            enemyAnimation.Initialize(enemyTexture, Vector2.Zero, 47, 61, 8, 30, Color.White, 1f, true);

//            // Randomly generate the position fo the enemy
//            Vector2 position = new Vector2(GraphicsDevice.Viewport.Width + enemyTexture.Width / 2,
//                                            random.Next(100, GraphicsDevice.Viewport.Height - 100));

//            // Create an enemy
//            enemy enemyI = new enemy();

//            // Initialize the enemy
//            enemyI.Initialize(enemyAnimation, position);

//            // Add the enemy to the active enemies list
//            enemies.Add(enemyI);
//        }

//        /// <summary>
//        /// This manages when to add and remove enemies and updates current enemies
//        /// </summary>
//        /// 
//        private void UpdateEnemies(GameTime gametime)
//        {
//            // Spawn a new enemey every 1.5 seconds
//            if (gametime.TotalGameTime - previousSpawnTime > enemySpawnTime)
//            {
//                previousSpawnTime = gametime.TotalGameTime;

//                // Add an Enemy
//                AddEnemy();
//            }

//            // Update the Enemies
//            for (int i = enemies.Count - 1; i >= 0; i--)
//            {
//                enemies[i].Update(gametime);

//                if (enemies[i].Active == false)
//                {
//                    // If not active and health <= 0
//                    if (enemies[i].Health <= 0)
//                    {
//                        // Add an explosion
//                        AddExplosion(enemies[i].Position);

//                        // Play the explosion sound
//                        explosionSound.Play();

//                        // Add to the player's score
//                        score += enemies[i].Value;
//                    }

//                    enemies.RemoveAt(i);
//                }
//            }
//        }

//        /// <summary>
//        /// This is called when the game should draw itself.
//        /// </summary>
//        /// <param name="gameTime">Provides a snapshot of timing values.</param>
//        protected override void Draw(GameTime gameTime)
//        {
//            GraphicsDevice.Clear(Color.CornflowerBlue);

//            // Start drawing
//            spriteBatch.Begin();

//            spriteBatch.Draw(mainBackground, Vector2.Zero, Color.White);

//            // Draw the moving background
//            bgLayer1.Draw(spriteBatch);
//            bgLayer2.Draw(spriteBatch);

//            // Draw the Player
//            player.Draw(spriteBatch);

//            // Draw the enemies
//            for (int i = 0; i < enemies.Count; i++)
//            {
//                enemies[i].Draw(spriteBatch);
//            }

//            // Draw the projectiles
//            for (int i = 0; i < projectiles.Count; i++)
//            {
//                projectiles[i].Draw(spriteBatch);
//            }

//            // Draw the explosions
//            for (int i = 0; i < explosions.Count; i++)
//            {
//                explosions[i].Draw(spriteBatch);
//            }

//            // Draw the score
//            spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X,
//                                    GraphicsDevice.Viewport.TitleSafeArea.Y), Color.White);
//            // Draw the player Health
//            spriteBatch.DrawString(font, "Health: " + player.Health, new Vector2(GraphicsDevice.Viewport.TitleSafeArea.X,
//                                    GraphicsDevice.Viewport.TitleSafeArea.Y + 30), Color.White);

//            // Stop drawing
//            spriteBatch.End();

//            base.Draw(gameTime);
//        }

//        private void UpdateCollision()
//        {
//            // Use the Rectangle's built-in intersect function to 
//            // determine if two objects are overlapping
//            Rectangle rectangle1;
//            Rectangle rectangle2;

//            // Only create the rectangle once for the player
//            rectangle1 = new Rectangle((int)player.Position.X, (int)player.Position.Y,
//                                        player.Width, player.Height);

//            // Do the collision between the player and the enemies
//            for (int i = 0; i < enemies.Count; i++)
//            {
//                rectangle2 = new Rectangle((int)enemies[i].Position.X, (int)enemies[i].Position.Y,
//                                            enemies[i].Width, enemies[i].Height);

//                // Determine if the two objects collided with each other
//                if (rectangle1.Intersects(rectangle2))
//                {
//                    // Subtract the health from the player based on the enemy damage
//                    player.Health -= enemies[i].Damage;

//                    // Since the enemy collided with the player destroy it
//                    enemies[i].Health = 0;

//                    // If the player health is less than zero we died
//                    if (player.Health <= 0)
//                        player.Active = false;
//                }
//            }

//            // Projectile vs Enemy Collision
//            for (int i = 0; i < projectiles.Count; i++)
//            {
//                for (int j = 0; j < enemies.Count; j++)
//                {
//                    // Create the rectangle we need to determine if we collided with each other
//                    rectangle1 = new Rectangle((int)projectiles[i].Position.X - projectiles[i].Width / 2,
//                                               (int)projectiles[i].Position.Y - projectiles[i].Height / 2,
//                                               projectiles[i].Width, projectiles[i].Height);

//                    rectangle2 = new Rectangle((int)enemies[j].Position.X - enemies[j].Width / 2,
//                                               (int)enemies[j].Position.Y - enemies[j].Height / 2,
//                                               enemies[j].Width, enemies[j].Height);

//                    // Determine if the two objects collided with each other
//                    if (rectangle1.Intersects(rectangle2))
//                    {
//                        enemies[j].Health -= projectiles[i].Damage;
//                        projectiles[i].Active = false;
//                    }
//                }
//            }
//        }

//        private void AddProjectile(Vector2 position)
//        {
//            Projectile projectile = new Projectile();
//            projectile.Initialize(GraphicsDevice.Viewport, projectileTexture, position);
//            projectiles.Add(projectile);
//        }

//        private void UpdateProjectiles()
//        {
//            // Update the Projectiles
//            for (int i = projectiles.Count - 1; i >= 0; i--)
//            {
//                projectiles[i].Update();

//                if (projectiles[i].Active == false)
//                {
//                    projectiles.RemoveAt(i);
//                }
//            }
//        }

//        private void AddExplosion(Vector2 position)
//        {
//            Animation explosion = new Animation();
//            explosion.Initialize(explosionTexture, position, 134, 134, 12, 45, Color.White, 1f, false);
//            explosions.Add(explosion);
//        }

//        private void UpdateExplosions(GameTime gametime)
//        {
//            for (int i = explosions.Count - 1; i >= 0; i--)
//            {
//                explosions[i].Update(gametime);
//                if (explosions[i].Active == false)
//                {
//                    explosions.RemoveAt(i);
//                }
//            }
//        }

//        private void PlayMusic(Song song)
//        {
//            // Due to the way the MediaPlayer plays music,
//            // we have to catch the exception. Music play when the game is not tethered
//            try
//            {
//                // Play the music
//                MediaPlayer.Play(song);

//                // Loop the currently playing song
//                MediaPlayer.IsRepeating = true;
//            }
//            catch { }
//        }

//    }
//}
