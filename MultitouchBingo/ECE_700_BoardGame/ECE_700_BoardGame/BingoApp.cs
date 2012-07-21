using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using System.Collections;

using ECE_700_BoardGame.Layout;
using ECE_700_BoardGame.Helper;
using ECE_700_BoardGame.Engine;
using System.Diagnostics;
using System.Data;
using ECE_700_BoardGame.Screens;

namespace ECE_700_BoardGame
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class BingoApp : Microsoft.Xna.Framework.Game
    {
        #region TemplatedFields

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        private TouchTarget touchTarget;
        private Color backgroundColor = new Color(81, 81, 81);
        private bool applicationLoadCompleteSignalled;

        private UserOrientation currentOrientation = UserOrientation.Bottom;
        private Matrix screenTransform = Matrix.Identity;

        /// <summary>
        /// The target receiving all surface input for the application.
        /// </summary>
        protected TouchTarget TouchTarget
        {
            get { return touchTarget; }
        }

        #endregion

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BingoApp()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #region Initialization

        /// <summary>
        /// Moves and sizes the window to cover the input surface.
        /// </summary>
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;

            // Get the window sized right.
            Program.InitializeWindow(Window);
            // Set the graphics device buffers.
            graphics.PreferredBackBufferWidth = Program.WindowSize.Width;
            graphics.PreferredBackBufferHeight = Program.WindowSize.Height;
            graphics.ApplyChanges();
            // Make sure the window is in the right location.
            Program.PositionWindow();
        }

        /// <summary>
        /// Initializes the surface input system. This should be called after any window
        /// initialization is done, and should only be called once.
        /// </summary>
        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero,
                "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
                return;
            System.Diagnostics.Debug.Assert(touchTarget == null,
                "Surface input already initialized");
            if (touchTarget != null)
                return;

            // Create a target for surface input.
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();
        }

        #endregion

        #region GameFields

        MouseState Mouse_State;
        MouseState Mouse_PrevState;

        ReadOnlyTouchPointCollection Touches;
        ReadOnlyTouchPointCollection TouchesPrevState;

        //PlayerOne = TopLeft, Players Numbered Clockwise
        

        int screenWidth;
        int screenHeight;
        int boardWidth;

        //Background Elements. All content sits on top of this content
        BackgroundItem BackgroundBase;
        List<ParallaxingBackground> ParallaxingBacking;

        ECE_700_BoardGame.Screens.Screen CurrentScreen;
        bool QuestionChanged = false;

        DatabaseHelper dbhelper = DatabaseHelper.Instance;

        #endregion

        #region Overridden Game Methods

        /// <summary>
        /// Allows the app to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ParallaxingBacking = new List<ParallaxingBackground>();        

            #region TemplatedInitialize

            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Set the application's orientation based on the orientation at launch
            currentOrientation = ApplicationServices.InitialOrientation;

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            // Setup the UI to transform if the UI is rotated.
            // Create a rotation matrix to orient the screen so it is viewed correctly
            // when the user orientation is 180 degress different.
            Matrix inverted = Matrix.CreateRotationZ(MathHelper.ToRadians(180)) *
                       Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                 graphics.GraphicsDevice.Viewport.Height,
                                                 0);

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }

            base.Initialize();

            #endregion

            #region Event Handlers

            Mouse_State = Mouse_PrevState = Mouse.GetState();
            Touches = TouchesPrevState = TouchTarget.GetState();
            //TouchesPrevState = TouchTarget.GetState();
            //Touches =  TouchTarget.GetState();

            #endregion
        }


        /// <summary>
        /// LoadContent will be called once per app and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            screenWidth = GraphicsDevice.Viewport.Width;
            screenHeight = GraphicsDevice.Viewport.Height;

#if DEBUG
            Debug.WriteLine(screenWidth.ToString() + " " + screenHeight.ToString(), "Screen Width, Height");
#endif

            #region Position Main Background Tiles

            Texture2D backTex = Content.Load<Texture2D>("BingoEnvironment/Bingo_BlueBack");
            Vector2 originBack = new Vector2(backTex.Width / 2, backTex.Height / 2);
            Rectangle posRect = new Rectangle(screenWidth / 2, screenHeight / 2, screenWidth, screenHeight);

            //Path, RectDestination, Orientation, ContentManager
            BackgroundBase = new BackgroundItem(backTex, posRect, 0, originBack);

            #endregion

            #region ParallaxingElements

            Rectangle paraRect = new Rectangle(0, 0, screenWidth, screenHeight);
            int spacing = screenWidth * 2;

            ParallaxingBackground sun = new ParallaxingBackground();
            sun.Initialize(Content, "BingoEnvironment/Sun", screenWidth, spacing, paraRect, -0.1f);
            ParallaxingBacking.Add(sun);

            ParallaxingBackground kite = new ParallaxingBackground();
            kite.Initialize(Content, "BingoEnvironment/Kite", screenWidth, spacing, paraRect, -0.15f);
            ParallaxingBacking.Add(kite);

            spacing = screenWidth + 200;
            ParallaxingBackground cloudLarge = new ParallaxingBackground();
            cloudLarge.Initialize(Content, "BingoEnvironment/CloudLarge", screenWidth, spacing, paraRect, -0.2f);
            ParallaxingBacking.Add(cloudLarge);

            ParallaxingBackground cloudMedium = new ParallaxingBackground();
            cloudMedium.Initialize(Content, "BingoEnvironment/CloudMedium", screenWidth, spacing, paraRect, -0.25f);
            ParallaxingBacking.Add(cloudMedium);

            ParallaxingBackground cloudsSmall = new ParallaxingBackground();
            cloudsSmall.Initialize(Content, "BingoEnvironment/CloudsSmall", screenWidth, spacing, paraRect, -0.3f);
            ParallaxingBacking.Add(cloudsSmall);

            #endregion

            CurrentScreen = new MenuScreen(this, spriteBatch, screenHeight, screenWidth);
            CurrentScreen.LoadContent(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per app and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public ECE_700_BoardGame.Screens.Screen GetGameState()
        {
            return CurrentScreen;
        }

        /// <summary>
        /// Allows the app to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (ApplicationServices.WindowAvailability != WindowAvailability.Unavailable)
            {
                if (ApplicationServices.WindowAvailability == WindowAvailability.Interactive)
                {
                    #region Touch Events

                    Touches = touchTarget.GetState();
                    CurrentScreen.Update(gameTime, Touches);
                                        
                    base.Update(gameTime);
                         
                    #endregion

                    #region Mouse Events

#if DEBUG
                    Mouse_State = Mouse.GetState();
                    if (Mouse_State != Mouse_PrevState)
                    {
                        Debug.WriteLine(Mouse_State.X.ToString(), "Mouse X Position");
                        Debug.WriteLine(Mouse_State.Y.ToString(), "Mouse Y Position");

                        CurrentScreen.Update(gameTime, Mouse_State);
                        
                        Mouse_PrevState = Mouse_State;
                    }
#endif
                    #endregion
                }
            }
        }

        /// <summary>
        /// This is called when the app should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (!applicationLoadCompleteSignalled)
            {
                // Dismiss the loading screen now that we are starting to draw
                ApplicationServices.SignalApplicationLoadComplete();
                applicationLoadCompleteSignalled = true;
            }

            GraphicsDevice.Clear(backgroundColor);

            spriteBatch.Begin();

            BackgroundBase.Draw(spriteBatch);

            foreach (ParallaxingBackground pb in ParallaxingBacking)
            {
                pb.Update();
                pb.Draw(spriteBatch);
            }

            CurrentScreen.Draw(gameTime);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        public void StartGame(List<String> topics, GameDifficulty difficulty)
        {
            CurrentScreen = new GameScreen(this, spriteBatch, screenHeight, screenWidth, topics, difficulty);
            CurrentScreen.LoadContent(Content);
        }

        #region Application Event Handlers

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: Enable audio, animations here

            //TODO: Optionally enable raw image here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: Optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: Disable audio, animations here

            //TODO: Disable raw image if it's enabled
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources.
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (touchTarget != null)
                {
                    touchTarget.Dispose();
                    touchTarget = null;
                }
            }

            // Release unmanaged Resources.

            // Set large objects to null to facilitate garbage collection.

            dbhelper.disconnectDB();
            base.Dispose(disposing);
        }

        #endregion
    }
}
