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
        private bool hasSetOptions = false;

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

            bool QuestionChanged = false;
            TimeSpan QuestionLastChanged; 
            private const int DIVIDER_THICKNESS = 60;

            //PlayerOne = TopLeft, Players Numbered Clockwise
            private const int PLAYER_COUNT = 4;
            private const int BOARD_TILE_WIDTH = 4;

            int screenWidth;
            int screenHeight;
            int boardWidth;

            //Background Elements. All content sits on top of this content
            BackgroundItem BackgroundBase;
            BackgroundItem PlayerColors;
            List<ParallaxingBackground> ParallaxingBacking;

            //Bingo Grids That tiles sit amongst
            BackgroundItem BingoGridOne;
            BackgroundItem BingoGridTwo;
            BackgroundItem BingoGridThree;
            BackgroundItem BingoGridFour;
            List<BackgroundItem> BingoBoards;

            //Dividers Between Players 
            List<BackgroundItem> Dividers;

            //Question tile in centre
            ECE_700_BoardGame.Engine.QuestionButton Question;
            BackgroundItem QuestionArea;

            //Player answer tiles
            List<BingoTile>[] PlayerTiles;

            Player[] PlayerData;

            Hashtable PossibleQuestions;

            List<String> Topics;
            List<SettingButton> SettingButtons;

            GameDifficulty Difficulty = GameDifficulty.Easy;
            ContinueButton PlayButton;

            DatabaseHelper dbhelper = new DatabaseHelper();

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

            BingoBoards         = new List<BackgroundItem>();
            Dividers            = new List<BackgroundItem>();
            ParallaxingBacking  = new List<ParallaxingBackground>();

            PlayerTiles = new List<BingoTile>[PLAYER_COUNT];
            PlayerData = new Player[PLAYER_COUNT];

            for(int i=0; i<PLAYER_COUNT; i++){
                PlayerTiles[i] = new List<BingoTile>();
            }

            Topics = new List<String>();
            
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
            Vector2 originBack = new Vector2(backTex.Width/2, backTex.Height/2);
            Rectangle posRect = new Rectangle(screenWidth/2, screenHeight/2, screenWidth, screenHeight);

            //Path, RectDestination, Orientation, ContentManager
            BackgroundBase = new BackgroundItem(backTex, posRect, 0, originBack);

            Texture2D playerColorTex = Content.Load<Texture2D>("BingoEnvironment/Bingo_PlayerColours");
            PlayerColors = new BackgroundItem(playerColorTex, posRect, 0, originBack);

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

            spacing = screenWidth+200;
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

            #region Position Bingo Boards

            Texture2D boardTex = Content.Load<Texture2D>("BingoEnvironment/BingoBoard");
            boardWidth = Convert.ToInt16(screenHeight/2.3);
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

            Texture2D dividerTex = Content.Load<Texture2D>("BingoEnvironment/BingoDivider");

            //Horizontals
            Rectangle posDivider = new Rectangle(0, (screenHeight / 2) - (DIVIDER_THICKNESS / 2), screenWidth / 3, DIVIDER_THICKNESS);

            BackgroundItem P1P4_Divider = new BackgroundItem(dividerTex, posDivider, 0);

            posDivider.X = screenWidth;
            posDivider.Y += DIVIDER_THICKNESS;
            BackgroundItem P2P3_Divider = new BackgroundItem(dividerTex, posDivider, MathHelper.Pi);

            //Verticals
            posDivider = new Rectangle((screenWidth/2) + (DIVIDER_THICKNESS/2), 0, screenWidth / 5 - 80, DIVIDER_THICKNESS);

            BackgroundItem P1P2_Divider = new BackgroundItem(dividerTex, posDivider, MathHelper.Pi / 2);

            posDivider.Y = screenHeight;
            posDivider.X -= DIVIDER_THICKNESS;
            BackgroundItem P3P4_Divider = new BackgroundItem(dividerTex, posDivider, (3 * MathHelper.Pi) / 2);

            Dividers.Add(P1P4_Divider);
            Dividers.Add(P2P3_Divider);
            Dividers.Add(P1P2_Divider); //top
            Dividers.Add(P3P4_Divider); //bottom

            #endregion
            
            #region Game Settings
            // Display topics
            DataTable dt = this.dbhelper.queryDBRows("select Topic from Topics");
            SettingButtons = new List<SettingButton>();
            int y = screenHeight / 2 - dt.Rows.Count * 50;
            
            Texture2D tex;
            Rectangle pos;
            foreach (DataRow row in dt.Rows)
            {
                String topic = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + topic);
                pos = new Rectangle(screenWidth / 8, y, tex.Width, tex.Height);
                SettingButtons.Add(new SettingButton(this, tex, pos, "TOPIC", topic));
                y += 100;
            }
            // Display difficulties
            dt = this.dbhelper.queryDBRows("select Difficulty from DifficultyLevels");
            y = screenHeight / 2 - dt.Rows.Count * 50;
            foreach (DataRow row in dt.Rows)
            {
                String diff = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + diff);
                pos = new Rectangle(screenWidth * 5 / 8, y, tex.Width, tex.Height);
                SettingButton sb = new SettingButton(this, tex, pos, "DIFFICULTY", diff);
                if (diff.Equals("Easy"))
                {
                    sb.Selected = true;
                }
                SettingButtons.Add(sb);
                y += 100;
            }

            tex = Content.Load<Texture2D>("BingoEnvironment/Play");
            pos = new Rectangle(screenWidth / 2, y, tex.Width, tex.Height);
            PlayButton = new ContinueButton(this, tex, pos);
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
                        if (sameTouch != null)
                        {
                            continue;
                        }

                        if (!this.hasSetOptions)
                        {
                            // Check for settings changed
                            foreach (SettingButton b in this.SettingButtons)
                            {
                                b.OnTouchTapGesture(touch);
                            }
                            PlayButton.OnTouchTapGesture(touch);
                            continue;
                        }
                        
                        TagData td = touch.Tag;
                        if ((td.Value == 0xC0 || td.Value == 8) && !this.QuestionChanged)
                        {
                            // Enable question changing
                            Question.Enabled = true;
                            this.QuestionChanged = true;
                            QuestionLastChanged = gameTime.TotalGameTime;
                        }

                        
                        //Check for tile touched
                        for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                        {
                            int tileNum = 0;
                            foreach (BingoTile bt in PlayerTiles[playerIndex])
                            {
                                bt.OnTouchTapGesture(touch);
                                PlayerData[playerIndex].tileAnswered(bt.Answered, tileNum);
                                tileNum++;
                            }
                        }

                        //Check for question touched
                        if (Question.Enabled && Question.OnTouchTapGesture(touch))
                        {
                            int questionID = Question.getID();

                            //Notify tiles of new question ID
                            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                            {
                                foreach (BingoTile bt in PlayerTiles[playerIndex])
                                {
                                    // Get possible answer images
                                    DataTable dt = dbhelper.queryDBRows("select ImageID from Answers where QuestionID = " + questionID.ToString());
                                    List<int> list = new List<int>();
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        list.Add(Int32.Parse(dt.Rows[i].ItemArray[0].ToString()));
                                    }
                                    bt.Update(list);
                                }
                            }
                        }
                    }
                    TouchesPrevState = Touches;
                    QuestionChanged = false;
                    #endregion

                    #region Mouse Events

#if DEBUG
                    Mouse_State = Mouse.GetState();
                    if (Mouse_State != Mouse_PrevState)
                    {
                        Debug.WriteLine(Mouse_State.X.ToString(), "Mouse X Position");
                        Debug.WriteLine(Mouse_State.Y.ToString(), "Mouse Y Position");

                        if (!this.hasSetOptions)
                        {
                            // Check for settings changed
                            foreach (SettingButton b in this.SettingButtons)
                            {
                                b.OnClickGesture(Mouse_State);
                            }
                            PlayButton.OnClickGesture(Mouse_State);
                        }
                        else
                        {

                            //Check for tile clicked
                            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                            {
                                int tileNum = 0;
                                foreach (BingoTile bt in PlayerTiles[playerIndex])
                                {
                                    bt.ClickEvent(Mouse_State);
                                    PlayerData[playerIndex].tileAnswered(bt.Answered, tileNum);
                                    tileNum++;
                                }
                            }

                            //Check for question clicked
                            if (Question.Enabled && Question.OnClickGesture(Mouse_State))
                            {
                                int questionID = Question.getID();

                                //Notify tiles of new question ID
                                for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                                {
                                    // Get possible answer images
                                    DataTable dt = dbhelper.queryDBRows("select ImageID from Answers where QuestionID = " + questionID.ToString());

                                    foreach (BingoTile bt in PlayerTiles[playerIndex])
                                    {
                                        List<int> list = new List<int>();
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            list.Add(Int32.Parse(dt.Rows[i].ItemArray[0].ToString()));
                                        }
                                        bt.Update(list);
                                    }
                                }
                            }
                        }
                    Mouse_PrevState = Mouse_State;
                    }
#endif
                    #endregion
                }
            }

            if (Question != null)
                Question.Update(gameTime);
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

            GraphicsDevice.Clear(backgroundColor);

            spriteBatch.Begin();

            BackgroundBase.Draw(spriteBatch);

            foreach (ParallaxingBackground pb in ParallaxingBacking)
            {
                pb.Update();
                pb.Draw(spriteBatch);
            }

            // Draw start-up screen for the first time called
            if (!hasSetOptions)
            {
                // Display topic options
                //int screenWidth = GraphicsDevice.Viewport.Width;
                //int screenHeight = GraphicsDevice.Viewport.Height;
                int y = screenHeight / 2 - SettingButtons.Count * 50;
                String categories = "Categories:";
                SpriteFont font = Content.Load<SpriteFont>("Comic");
                String difficulty = "Difficulty levels:";
                spriteBatch.DrawString(font, categories, new Vector2(screenWidth / 8, y - 100), Color.Black,
                    0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
                spriteBatch.DrawString(font, difficulty, new Vector2(screenWidth * 5 / 8, y - 100), Color.Black,
                    0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
            
                foreach (SettingButton b in SettingButtons)
                {
                    b.Draw(spriteBatch);
                }

                // OK button
                PlayButton.Draw(spriteBatch);
            }
            else
            {
                PlayerColors.Draw(spriteBatch);

                foreach (BackgroundItem bi in BingoBoards)
                {
                    bi.Draw(spriteBatch);
                }

                foreach (BackgroundItem bi in Dividers)
                {
                    bi.Draw(spriteBatch);
                }

                QuestionArea.Draw(spriteBatch);
                Question.Draw(spriteBatch, gameTime);


                for (int playerCount = 0; playerCount < PLAYER_COUNT; playerCount++)
                {
                    foreach (BingoTile bt in PlayerTiles[playerCount])
                    {
                        bt.Draw(spriteBatch);
                    }
                }

                foreach (Player player in PlayerData)
                {
                    player.Draw(spriteBatch, GraphicsDevice);
                }
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        #endregion

        #region Options Setting
        public void AddSetting(String setting, String value)
        {
            switch (setting)
            {
                case "TOPIC":
                    if (!Topics.Contains(value))
                        this.Topics.Add(value);
                    break;
                case "DIFFICULTY":
                    // Ensure that only one level is selected at any point in time
                    var result = from b in SettingButtons
                                 where b.Setting.Equals(setting)
                                 select b;
                    foreach (SettingButton b in result)
                    {
                        // If button is not the selected difficulty and has been selected, deselect
                        if (!b.Value.Equals(value))
                        {
                            b.Selected = false;
                        }
                        else
                        {
                            b.Selected = true; // Otherwise "undo" press
                        }
                    }
                    if (value.Equals("Hard"))
                    {
                        this.Difficulty = GameDifficulty.Hard;
                    }
                    else
                    {
                        this.Difficulty = GameDifficulty.Easy;
                    }
                    break;
                default:
                    return;
            }            
        }

        public void RemoveSetting(String setting, String value)
        {
            switch (setting)
            {
                case "TOPIC":
                    this.Topics.Remove(value);
                    break;
                case "DIFFICULTY":
                    // Ensure that only one level is selected at any point in time
                    var result = from b in SettingButtons
                                 where b.Setting.Equals(setting)
                                 select b;
                    foreach (SettingButton b in result)
                    {
                        // If button is not the selected difficulty and has been selected, deselect
                        if (!b.Value.Equals(value))
                        {
                            b.Selected = false;
                        }
                        else
                        {
                            b.Selected = true; // Otherwise "undo" press
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Sets the difficulty level of questions/answers - default is easy
        /// </summary>
        /// <param name="difficulty"></param>
        public void SetDifficulty(int difficulty)
        {
            switch (difficulty) {
                case (1):
                    Difficulty = GameDifficulty.Easy;
                    break;
                case (2):
                    Difficulty = GameDifficulty.Hard;
                    break;
                default:
                    Difficulty = GameDifficulty.Easy;
                    break;
            }
        }

        public void FinishedSettingOptions()
        {
            #region Answer Tiles
            int Width = GraphicsDevice.Viewport.Width;
            int Height = GraphicsDevice.Viewport.Height;
            int bdWidth = Convert.ToInt16(screenHeight / 2.3);

            string tileAnswersQuery;
            //List<int> possibleQuestions = new List<int>();
            PossibleQuestions = new Hashtable();
            string difficulty = "";
            if (Difficulty.Equals(GameDifficulty.Easy))
            {
                difficulty = "and Difficulty = 1";
            }
            if (Topics.Count == 3 || Topics.Count == 0)
            {
                tileAnswersQuery = "select distinct Questions.QuestionID, Questions.Question, Answers.ImageID from Questions inner join Answers on Questions.QuestionID = Answers.QuestionID " + difficulty;
            }
            else
            {
                string topic = dbhelper.getQueryClause("Topic", Topics);

                tileAnswersQuery = "select Questions.QuestionID, Questions.Question, Answers.ImageID from Topics, Questions " +
                    "inner join Answers on Questions.QuestionID = Answers.QuestionID where Topics.TopicID = Questions.TopicID and " + topic + difficulty;
            }

            DataTable dt = dbhelper.queryDBRows(tileAnswersQuery);

            //Initialize to top left tile position for player 1
            Rectangle posRectAns = new Rectangle((screenWidth / 4) - (boardWidth / 2) + (boardWidth / 25),
                                                    (screenHeight / 4) - (boardWidth / 2) + (boardWidth / 20),
                                                    boardWidth / 6, boardWidth / 6);
            Texture2D daubTex = Content.Load<Texture2D>("daub");
            Texture2D errorTileTex = Content.Load<Texture2D>("error");

            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
            {
                switch (playerIndex)
                {
                    //Player 2
                    case (1):
                        posRectAns.X = ((screenWidth * 3) / 4) - (boardWidth / 2) + (boardWidth / 25);
                        posRectAns.Y = (screenHeight / 4) - (boardWidth / 2) + (boardWidth / 20);
                        break;

                    //Player 3
                    case (2):
                        posRectAns.X = (screenWidth / 4) - (boardWidth / 2) + (boardWidth / 25);
                        posRectAns.Y = ((screenHeight * 3) / 4) - (boardWidth / 2) + (boardWidth / 20);
                        break;

                    //Player 4
                    case (3):
                        posRectAns.X = ((screenWidth * 3) / 4) - (boardWidth / 2) + (boardWidth / 25);
                        posRectAns.Y = ((screenHeight * 3) / 4) - (boardWidth / 2) + (boardWidth / 20);
                        break;
                }

                List<int> answerIndex = new List<int>();
                List<int> answerTileImages = new List<int>();
                while (answerIndex.Count < 16)
                {
                    int rand = new Random().Next(dt.Rows.Count);
                    while (!answerIndex.Contains(rand))
                    {
                        object[] row = dt.Rows[rand].ItemArray;
                        int answerImageID = Int32.Parse(row[2].ToString());
                        if (answerTileImages.Contains(answerImageID))
                        {
                            rand = new Random().Next(dt.Rows.Count);
                            continue;
                        }
                        else
                        {
                            answerIndex.Add(rand);
                            answerTileImages.Add(answerImageID);
                            break;
                        }
                    }
                }

                if (playerIndex < (PLAYER_COUNT / 2))
                {
                    //posRectAns.X += posRectAns.Width;
                    //posRectAns.Y += posRectAns.Height;
                }

                int i = 0;
                foreach (var tileAnswer in answerTileImages)
                {
                    string filename = dbhelper.stringQueryDB("select Path from Answers, Images where Answers.ImageID = " + tileAnswer.ToString() +
                        " and Answers.ImageID = Images.ImageID");
                    Texture2D tileAnsTex = Content.Load<Texture2D>("QuestionAnswerImages/" + filename);

                    //Shift Tile Position
                    if (i != 0)
                    {
                        posRectAns.X += boardWidth / BOARD_TILE_WIDTH;

                        if ((i % BOARD_TILE_WIDTH) == 0)
                        {
                            posRectAns.X -= BOARD_TILE_WIDTH * (boardWidth / 4);
                            posRectAns.Y += boardWidth / BOARD_TILE_WIDTH;
                        }
                    }

                    BingoTile bt;
                    if (playerIndex < (PLAYER_COUNT / 2))
                    {
                        bt = new BingoTile(this, tileAnsTex, daubTex, errorTileTex, posRectAns, (float)Math.PI, new Vector2(tileAnsTex.Width, tileAnsTex.Height));
                    }
                    else
                    {
                        bt = new BingoTile(this, tileAnsTex, daubTex, errorTileTex, posRectAns);
                    }

                    bt.Initialize((int)tileAnswer);
                    PlayerTiles[playerIndex].Add(bt);

                    // Store all possible questions for the answer tiles in a question pool to be used by QuestionButton
                    DataTable questionIds = dbhelper.queryDBRows(
                        "select Questions.QuestionID from Questions, Answers where Questions.QuestionID = Answers.QuestionID and Answers.ImageID = "
                        + tileAnswer.ToString());
                    for (int j = 0; j < questionIds.Rows.Count; j++)
                    {
                        Int32 qId = Int32.Parse(questionIds.Rows[j].ItemArray[0].ToString());
                        Int32 freq = 1;
                        if (PossibleQuestions.Contains(qId))
                        {
                            freq = Int32.Parse(PossibleQuestions[qId].ToString());
                            freq++;
                            PossibleQuestions.Remove(qId);
                        }
                        PossibleQuestions.Add(qId, freq);
                    }

                    i++;
                }
            }

            #endregion

            #region Player Data

            for (int i = 0; i < PLAYER_COUNT; i++)
            {
                PlayerData[i] = new Player(PlayerTiles[i], i, this);
            }

            #endregion

            #region Question Tile

            Texture2D questionTex = Content.Load<Texture2D>("QuestionAnswerImages/Question");
            Rectangle questionPos = new Rectangle(screenWidth / 2, screenHeight / 2, questionTex.Width, questionTex.Height);
            Question = new QuestionButton(this, questionTex, questionPos, Topics, new ArrayList(PossibleQuestions.Keys), dbhelper);

            // Notify all bingo tiles that a question has been set
            DataTable answerImages = dbhelper.queryDBRows("select ImageID from Answers where QuestionID = " + Question.getID().ToString());
            List<int> list = new List<int>();
            for (int j = 0; j < answerImages.Rows.Count; j++)
            {
                list.Add(Int32.Parse(answerImages.Rows[j].ItemArray[0].ToString()));
            }
            foreach (List<BingoTile> pt in PlayerTiles)
            {
                foreach (BingoTile bt in pt)
                {
                    bt.Update(list);
                }
            }

            Texture2D quAreaTex = Content.Load<Texture2D>("BingoEnvironment/BingoQuestionMark_Area");
            Vector2 originQuArea = new Vector2(quAreaTex.Width / 2, quAreaTex.Height / 2);
            Rectangle posQuArea = new Rectangle(screenWidth / 2, screenHeight / 2, screenWidth, screenHeight);
            //Texture, RectDestination, Orientation, imagePositionOrigin
            QuestionArea = new BackgroundItem(quAreaTex, posQuArea, 0, originQuArea);

            #endregion

            Question.SelectQuestions(this.Topics);
            
            this.hasSetOptions = true;
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

            dbhelper.disconnectDB();
            base.Dispose(disposing);
        }

        #endregion
    }
}
