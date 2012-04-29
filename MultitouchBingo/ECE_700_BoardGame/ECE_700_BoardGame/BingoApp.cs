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

using ECE_700_BoardGame.Layout;
using ECE_700_BoardGame.Helper;
using ECE_700_BoardGame.Engine;
using System.Diagnostics;
using System.Data;

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

            ContentManager content;
        
            MouseState Mouse_State;
            MouseState Mouse_PrevState;

            ReadOnlyTouchPointCollection Touches;
            ReadOnlyTouchPointCollection TouchesPrevState;

            private const int DIVIDER_THICKNESS = 20;

            //PlayerOne = TopLeft, Players Numbered Clockwise
            private const int PLAYER_COUNT = 4;

            //Backgrounds All content sits on top of
            BackgroundItem PlayerOneBackground;
            BackgroundItem PlayerTwoBackground;
            BackgroundItem PlayerThreeBackground;
            BackgroundItem PlayerFourBackground;
            List<BackgroundItem> MainBacking;

            //Bingo Grids That tiles sit amongst
            BackgroundItem BingoGridOne;
            BackgroundItem BingoGridTwo;
            BackgroundItem BingoGridThree;
            BackgroundItem BingoGridFour;
            List<BackgroundItem> BingoBoards;

            //Dividers Between Players 
            BackgroundItem P1P4_Divider;
            BackgroundItem P2P3_Divider;
            BackgroundItem P1P2_Divider;
            BackgroundItem P3P4_Divider;
            List<BackgroundItem> Dividers;

            //Question tile in centre
            ECE_700_BoardGame.Engine.QuestionButton Question;

            //Player answer tiles
            List<BingoTile>[] PlayerTiles;

            Player[] PlayerData;     

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
            #region Lists

            MainBacking = new List<BackgroundItem>();
            BingoBoards = new List<BackgroundItem>();
            Dividers    = new List<BackgroundItem>();

            PlayerTiles = new List<BingoTile>[PLAYER_COUNT];
            PlayerData = new Player[PLAYER_COUNT];

            for(int i=0; i<PLAYER_COUNT; i++){
                PlayerTiles[i] = new List<BingoTile>();
            }
            //PlayerOneTiles  = new List<BingoTile>();
            //PlayerTwoTiles  = new List<BingoTile>();
            //PlayerThreeTiles= new List<BingoTile>();
            //PlayerFourTiles = new List<BingoTile>();
            
            #endregion

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

            //touchTarget.TouchTapGesture += Question.OnTouchTapGesture;
            Mouse_State = Mouse_PrevState = Mouse.GetState();
            //Touches = TouchesPrevState = TouchTarget.GetState();
            TouchesPrevState = TouchTarget.GetState();
            Touches =  TouchTarget.GetState();

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

            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

#if DEBUG
            Debug.WriteLine(screenWidth.ToString() + " " + screenHeight.ToString(), "Screen Width, Height");
#endif

            #region Position Main Background Tiles

            Texture2D backTex = Content.Load<Texture2D>("MountainBackground");
            Vector2 originBack = new Vector2(backTex.Width/2, backTex.Height/2);
            Rectangle posRect = new Rectangle(0 + screenWidth / 4, 0 + screenHeight / 4, screenWidth / 2, screenHeight / 2);

            //Path, RectDestination, Orientation, ContentManager
            PlayerOneBackground = new BackgroundItem(backTex, posRect, MathHelper.Pi, originBack);

            posRect.X += screenWidth / 2;
            //Added for positioning mirrored Imaged
            posRect.X += screenWidth / 4;
            posRect.Y += screenHeight/ 4;

            PlayerTwoBackground = new BackgroundItem(backTex, posRect, MathHelper.Pi, originBack, EnumSettings.ItemOrientation.RIGHT);

            //Added for positioning mirrored Imaged
            posRect.X -= screenWidth / 2;
            posRect.Y -= screenHeight / 2;

            posRect.X -= screenWidth / 2;
            posRect.Y += screenHeight / 2;
            PlayerThreeBackground = new BackgroundItem(backTex, posRect, 0, originBack, EnumSettings.ItemOrientation.RIGHT);

            //Added to undo mirrored Image positioning
            posRect.X += screenWidth / 4;
            posRect.Y += screenHeight / 4;

            posRect.X += screenWidth / 2;
            PlayerFourBackground = new BackgroundItem(backTex, posRect, 0, originBack);

            MainBacking.Add(PlayerOneBackground);
            MainBacking.Add(PlayerTwoBackground);
            MainBacking.Add(PlayerThreeBackground);
            MainBacking.Add(PlayerFourBackground);

            #endregion

            #region Position Bingo Boards

            Texture2D boardTex = Content.Load<Texture2D>("TempBingoBoard");
            int boardWidth = Convert.ToInt16(screenHeight/2.3);
            Rectangle posBoard = new Rectangle( (screenWidth / 4) - boardWidth/2,
                                                (screenHeight / 4) - boardWidth/2,
                                                boardWidth, boardWidth);
            //Vector2 originBoard = new Vector2(posBoard.Width / 2, posBoard.Height / 2);

#if DEBUG
            //NB Scaling effects how origin effects translation offset
            Debug.WriteLine(posBoard.X.ToString() + " " + posBoard.Y.ToString(), "Position Board");
            Debug.WriteLine(posBoard.Width.ToString() + " " + posBoard.Height.ToString(), "Board Size");
#endif
            BingoGridOne = new BackgroundItem(boardTex, posBoard, 0);
                                                                     
            posBoard.X += screenWidth / 2;         
            BingoGridTwo = new BackgroundItem(boardTex, posBoard, 0);
                                                                     
            posBoard.Y = screenHeight - (screenHeight / 4) - (boardWidth/2);       
            BingoGridThree = new BackgroundItem(boardTex, posBoard, 0);
                                                                     
            posBoard.X -= screenWidth / 2;         
            BingoGridFour = new BackgroundItem(boardTex, posBoard, 0);


            BingoBoards.Add(BingoGridOne);
            BingoBoards.Add(BingoGridTwo);
            BingoBoards.Add(BingoGridThree);
            BingoBoards.Add(BingoGridFour);

            #endregion

            #region Position Dividers

            Texture2D dividerTex = Content.Load<Texture2D>("Divider");

            //Horizontals
            Rectangle posDivider = new Rectangle(screenWidth/12, (screenHeight / 2) - (DIVIDER_THICKNESS / 2), screenWidth / 3, DIVIDER_THICKNESS);
            
            P1P4_Divider = new BackgroundItem(dividerTex, posDivider, 0);

            posDivider.X += screenWidth / 2;
            P2P3_Divider = new BackgroundItem(dividerTex, posDivider, 0);

            //Verticals
            posDivider = new Rectangle((screenWidth/2) + (DIVIDER_THICKNESS/2), screenHeight/12, screenWidth / 5, DIVIDER_THICKNESS);

            P1P2_Divider = new BackgroundItem(dividerTex, posDivider, MathHelper.Pi/2);

            posDivider.Y = screenHeight - (screenHeight/12) - posDivider.Width;
            P3P4_Divider = new BackgroundItem(dividerTex, posDivider, MathHelper.Pi/2);

            Dividers.Add(P1P4_Divider);
            Dividers.Add(P2P3_Divider);
            Dividers.Add(P1P2_Divider);
            Dividers.Add(P3P4_Divider);

            #endregion

            #region Question Tile
            Texture2D questionTex = Content.Load<Texture2D>("Question");
            Rectangle questionPos = new Rectangle(screenWidth / 2-questionTex.Width/2, screenHeight/2-questionTex.Height/2, questionTex.Width, questionTex.Height);
            Question = new QuestionButton(this, questionTex, questionPos, "Any");

            #endregion

            #region Answer Tiles

            string tileAnswersQuery;
            string tempTopic = "Any";

            if (tempTopic.Equals("Any"))
            {
                tileAnswersQuery = "select QuestionID, Question, HasImage from Questions";
            }
            else
            {
                tileAnswersQuery = "select QuestionID, Question, HasImage from Questions, Topics where Topics.TopicID = Questions.TopicID and Topic = '" + tempTopic + "'";
            }

            DataTable dt = Question.queryDBRows(tileAnswersQuery);

            //Initialize to top left tile position for player 1
            Rectangle posRectAns = new Rectangle(   (screenWidth / 4) - (boardWidth / 2)  + (boardWidth / 50),
                                                    (screenHeight / 4) - (boardWidth / 2) + (boardWidth / 35),
                                                    boardWidth / 7, boardWidth / 7);    
            Texture2D daubTex = Content.Load<Texture2D>("daub");
            Texture2D errorTileTex = Content.Load<Texture2D>("error");

            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
            {
                switch (playerIndex)
                {
                    //Player 2
                    case (1):
                        posRectAns.X = ((screenWidth * 3) / 4) - (boardWidth / 2) + (boardWidth / 50);
                        posRectAns.Y = (screenHeight / 4) - (boardWidth / 2) + (boardWidth / 35);
                        break;

                    //Player 3
                    case (2):
                        posRectAns.X = (screenWidth / 4) - (boardWidth / 2) + (boardWidth / 50);
                        posRectAns.Y = ((screenHeight * 3) / 4) - (boardWidth / 2) + (boardWidth / 35);
                        break;

                    //Player 4
                    case (3):
                        posRectAns.X = ((screenWidth * 3) / 4) - (boardWidth / 2) + (boardWidth / 50);
                        posRectAns.Y = ((screenHeight * 3) / 4) - (boardWidth / 2) + (boardWidth / 35);
                        break;
                }
                
                List<int> answerIndex = new List<int>();
                while (answerIndex.Count < 25)
                {
                    int rand = new Random().Next(dt.Rows.Count);
                    if(!answerIndex.Contains(rand)){
                        answerIndex.Add(rand);
                    }
                }

                int i = 0;
                foreach (var tileAnswer in answerIndex)
                {
                    object[] row = dt.Rows[tileAnswer].ItemArray;
                    int answerID = Int32.Parse(row[0].ToString());
                    
                    string filename = Question.stringQueryDB("select Path from Images where QuestionID = " + answerID.ToString());
                    Texture2D tileAnsTex = Content.Load<Texture2D>(filename);

                    //Shift Tile Position
                    if (i!=0)
                    {
                        posRectAns.X += boardWidth/5;

                        if ((i % 5) == 0)
                        {
                            posRectAns.X -= 5 * (boardWidth/5);
                            posRectAns.Y += boardWidth / 5;
                        }
                    }

                    BingoTile bt = new BingoTile(this, tileAnsTex, daubTex, errorTileTex, posRectAns);
                    bt.Initialize(answerID);
                    //bt.Update(13);
                    //touchTarget.TouchTapGesture += bt.OnTouchTapGesture;
                    PlayerTiles[playerIndex].Add(bt);

                    i++;
                }
            }

            #endregion

            #region Player Data

            PlayerData[1] = new Player(PlayerTiles[1], 1);

            #endregion
        }

        /// <summary>
        /// UnloadContent will be called once per app and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
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

                    foreach (TouchPoint touch in Touches)
                    {
                        var result = from oldtouch in TouchesPrevState
                                     from newtouch in Touches
                                     where Helper.Geometry.Contains(newtouch.Bounds, oldtouch.X, oldtouch.Y) &&
                                     newtouch.Id == oldtouch.Id
                                     select oldtouch;


                        var sameTouch = result.FirstOrDefault();
                        if(sameTouch != null){
                            int x = 14;
                            int y = x;
                            continue;
                        }

                        //Check for tile touched
                        for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                        {
                            int tileNum = 0;
                            foreach (BingoTile bt in PlayerTiles[playerIndex])
                            {
                                bt.OnTouchTapGesture(touch);
                                PlayerData[1].tileAnswered(bt.Answered, tileNum);
                                tileNum++;
                            }
                        }

                        //Check for question touched
                        if (Question.OnTouchTapGesture(touch))
                        {
                            int questionID = Question.getID();

                            //Notify tiles of new question ID
                            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                            {
                                foreach (BingoTile bt in PlayerTiles[playerIndex])
                                {
                                    bt.Update(questionID);
                                }
                            }
                        }
                    }
                    TouchesPrevState = Touches;

                    #endregion

                    #region Mouse Events

#if DEBUG
                    Mouse_State = Mouse.GetState();
                    if (Mouse_State != Mouse_PrevState)
                    {
                        Debug.WriteLine(Mouse_State.X.ToString(), "Mouse X Position");
                        Debug.WriteLine(Mouse_State.Y.ToString(), "Mouse Y Position");

                        //Check for tile clicked
                        for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                        {
                            int tileNum = 0;
                            foreach (BingoTile bt in PlayerTiles[playerIndex])
                            {
                                bt.ClickEvent(Mouse_State);
                                PlayerData[1].tileAnswered(bt.Answered, tileNum);
                                tileNum++;
                            }
                        }

                        //Check for question clicked
                        if (Question.OnClickGesture(Mouse_State))
                        {
                            int questionID = Question.getID();

                            //Notify tiles of new question ID
                            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                            {
                                foreach (BingoTile bt in PlayerTiles[playerIndex])
                                {
                                    bt.Update(questionID);
                                }
                            }
                        }
                    Mouse_PrevState = Mouse_State;
                    }
#endif
                    #endregion
                }
            }

            base.Update(gameTime);
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

            //TODO: Rotate the UI based on the value of screenTransform here if desired

            GraphicsDevice.Clear(backgroundColor);

            spriteBatch.Begin();

            foreach (BackgroundItem bi in MainBacking)
            {
                bi.Draw(spriteBatch);
            }

            foreach (BackgroundItem bi in BingoBoards)
            {
                bi.Draw(spriteBatch);
            }

            foreach (BackgroundItem bi in Dividers)
            {
                bi.Draw(spriteBatch);
            }

            Question.Draw(spriteBatch, gameTime);

            for (int playerCount = 0; playerCount < PLAYER_COUNT; playerCount++)
            {
                foreach (BingoTile bt in PlayerTiles[playerCount])
                {
                    bt.Draw(spriteBatch);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

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

            Question.disconnectDB();
            base.Dispose(disposing);
        }

        #endregion
    }
}