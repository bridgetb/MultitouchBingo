using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ECE_700_BoardGame.Engine;
using System.Data;
using ECE_700_BoardGame.Helper;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework.Input;
using ECE_700_BoardGame.Layout;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;

namespace ECE_700_BoardGame.Screens
{
    public class GameScreen : Screen
    {
        Game Game;
        int ScreenHeight;
        int ScreenWidth;
        SpriteBatch SpriteBatch;
        ContentManager Content;
        DatabaseHelper DBhelper = DatabaseHelper.Instance;
        ReadOnlyTouchPointCollection TouchesPrevState;
        TagData TagPrevState;


        //Bingo Grids That tiles sit amongst
        BackgroundItem BingoGridOne;
        BackgroundItem BingoGridTwo;
        BackgroundItem BingoGridThree;
        BackgroundItem BingoGridFour;
        List<BackgroundItem> BingoBoards;
        BackgroundItem PlayerColors;

        //Dividers Between Players 
        List<BackgroundItem> Dividers;

        //Question tile in centre
        ECE_700_BoardGame.Engine.QuestionButton Question;
        BackgroundItem QuestionArea;
        bool QuestionChanged;
        List<String> Topics;
        GameDifficulty Difficulty = GameDifficulty.Easy;

        //Player answer tiles
        List<BingoTile>[] PlayerTiles;

        //List<Animation> IncorrectAnswerDaub;
        //List<Animation> CorrectAnswerDaub;
        Texture2D IncorrectSpriteStrip;
        Texture2D CorrectSpriteStrip;

        Player[] PlayerData;

        private const int PLAYER_COUNT = 4;
        private int BOARD_TILE_WIDTH;
        private const int DIVIDER_THICKNESS = 60;

        public GameScreen(Game game, SpriteBatch spriteBatch, int screenHeight, int screenWidth, List<String> topics, 
            GameDifficulty difficulty)
        {
            this.Game = game;
            this.SpriteBatch = spriteBatch;
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth;
            Dividers = new List<BackgroundItem>();
            BingoBoards = new List<BackgroundItem>();
            PlayerTiles = new List<BingoTile>[PLAYER_COUNT];
            PlayerData = new Player[PLAYER_COUNT];
            this.Topics = topics;
            this.Difficulty = difficulty;

            for (int i = 0; i < PLAYER_COUNT; i++)
            {
                PlayerTiles[i] = new List<BingoTile>();
            }
        }

        public void LoadContent(ContentManager content)
        {
            Content = content;
            Texture2D backTex = Content.Load<Texture2D>("BingoEnvironment/Bingo_BlueBack");
            Vector2 originBack = new Vector2(backTex.Width / 2, backTex.Height / 2);
            Rectangle posRect = new Rectangle(ScreenWidth / 2, ScreenHeight / 2, ScreenWidth, ScreenHeight);
            Texture2D playerColorTex = Content.Load<Texture2D>("BingoEnvironment/Bingo_PlayerColours");
            PlayerColors = new BackgroundItem(playerColorTex, posRect, 0, originBack);

            CorrectSpriteStrip = Content.Load<Texture2D>("BingoEnvironment/GrinStrip");
            IncorrectSpriteStrip = Content.Load<Texture2D>("BingoEnvironment/SurpriseStrip");

            #region Position Bingo Boards
            Texture2D boardTex;
            if (this.Difficulty == GameDifficulty.Easy)
            {
                boardTex = Content.Load<Texture2D>("BingoEnvironment/BingoBoard3x3");
                BOARD_TILE_WIDTH = 3;
            }
            else
            {
                boardTex = Content.Load<Texture2D>("BingoEnvironment/BingoBoard4x4");
                BOARD_TILE_WIDTH = 4;
            }
            int boardWidth = Convert.ToInt16(ScreenHeight / 2.3);
            Rectangle posBoard = new Rectangle((ScreenWidth / 4) - boardWidth / 2,
                                                (ScreenHeight / 4) - boardWidth / 2,
                                                boardWidth, boardWidth);

#if DEBUG
            //NB Scaling effects how origin effects translation offset
            Debug.WriteLine(posBoard.X.ToString() + " " + posBoard.Y.ToString(), "Position Board");
            Debug.WriteLine(posBoard.Width.ToString() + " " + posBoard.Height.ToString(), "Board Size");
#endif
            BingoGridOne = new BackgroundItem(boardTex, posBoard, 0);

            posBoard.X += ScreenWidth / 2;
            BingoGridTwo = new BackgroundItem(boardTex, posBoard, 0);

            posBoard.Y = ScreenHeight - (ScreenHeight / 4) - (boardWidth / 2);
            BingoGridThree = new BackgroundItem(boardTex, posBoard, 0);

            posBoard.X -= ScreenWidth / 2;
            BingoGridFour = new BackgroundItem(boardTex, posBoard, 0);


            BingoBoards.Add(BingoGridOne);
            BingoBoards.Add(BingoGridTwo);
            BingoBoards.Add(BingoGridThree);
            BingoBoards.Add(BingoGridFour);

            #endregion

            #region Position Dividers

            Texture2D dividerTex = Content.Load<Texture2D>("BingoEnvironment/BingoDivider");

            //Horizontals
            Rectangle posDivider = new Rectangle(0, (ScreenHeight / 2) - (DIVIDER_THICKNESS / 2), ScreenWidth / 3, DIVIDER_THICKNESS);

            BackgroundItem P1P4_Divider = new BackgroundItem(dividerTex, posDivider, 0);

            posDivider.X = ScreenWidth;
            posDivider.Y += DIVIDER_THICKNESS;
            BackgroundItem P2P3_Divider = new BackgroundItem(dividerTex, posDivider, MathHelper.Pi);

            //Verticals
            posDivider = new Rectangle((ScreenWidth / 2) + (DIVIDER_THICKNESS / 2), 0, ScreenWidth / 5 - 80, DIVIDER_THICKNESS);

            BackgroundItem P1P2_Divider = new BackgroundItem(dividerTex, posDivider, MathHelper.Pi / 2);

            posDivider.Y = ScreenHeight;
            posDivider.X -= DIVIDER_THICKNESS;
            BackgroundItem P3P4_Divider = new BackgroundItem(dividerTex, posDivider, (3 * MathHelper.Pi) / 2);

            Dividers.Add(P1P4_Divider);
            Dividers.Add(P2P3_Divider);
            Dividers.Add(P1P2_Divider); //top
            Dividers.Add(P3P4_Divider); //bottom

            #endregion

            #region Question Tile

            Texture2D questionTex = Content.Load<Texture2D>("QuestionAnswerImages/Question"); // Dummy question image
            Rectangle questionPos = new Rectangle(ScreenWidth / 2, ScreenHeight / 2, questionTex.Width, questionTex.Height);
            Question = new QuestionButton(Game, questionTex, questionPos, questionPos, Topics, DBhelper);

            #endregion

            #region Answer Tiles
            string tileAnswersQuery;

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
                string topic = DBhelper.getQueryClause("Topic", Topics);

                tileAnswersQuery = "select Questions.QuestionID, Questions.Question, Answers.ImageID from Topics, Questions " +
                    "inner join Answers on Questions.QuestionID = Answers.QuestionID where Topics.TopicID = Questions.TopicID and " + topic + difficulty;
            }

            DataTable dt = DBhelper.queryDBRows(tileAnswersQuery);

            //Initialize to top left tile position for player 1
            int ansTileLength = (int)(boardWidth / (BOARD_TILE_WIDTH * 1.5));
            int gapInTile = (int)(boardWidth / BOARD_TILE_WIDTH * 0.25 * 0.75);
            Rectangle posRectAns = new Rectangle((ScreenWidth / 4) - (boardWidth / 2) + gapInTile,
                                                    (ScreenHeight / 4) - (boardWidth / 2) + gapInTile,
                                                    ansTileLength, ansTileLength);

            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
            {
                switch (playerIndex)
                {
                    //Player 2
                    case (1):
                        //posRectAns.X = ((ScreenWidth * 3) / 4) - (boardWidth / 2) + (boardWidth / 25);
                        posRectAns.X = ((ScreenWidth * 3) / 4) - (boardWidth / 2) + gapInTile;
                        posRectAns.Y = (ScreenHeight / 4) - (boardWidth / 2) + gapInTile;
                        break;

                    //Player 3
                    case (2):
                        //posRectAns.X = (ScreenWidth / 4) - (boardWidth / 2) + (boardWidth / 25);
                        posRectAns.X = (ScreenWidth / 4) - (boardWidth / 2) + gapInTile;
                        posRectAns.Y = ((ScreenHeight * 3) / 4) - (boardWidth / 2) + gapInTile;
                        break;

                    //Player 4
                    case (3):
                        //posRectAns.X = ((ScreenWidth * 3) / 4) - (boardWidth / 2) + (boardWidth / 25);
                        posRectAns.X = ((ScreenWidth * 3) / 4) - (boardWidth / 2) + gapInTile;
                        posRectAns.Y = ((ScreenHeight * 3) / 4) - (boardWidth / 2) + gapInTile;
                        break;
                }

                List<int> answerIndex = new List<int>();
                List<int> answerTileImages = new List<int>();
                while (answerIndex.Count < BOARD_TILE_WIDTH * BOARD_TILE_WIDTH)
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
                    string filename = DBhelper.stringQueryDB("select Path from Answers, Images where Answers.ImageID = " + tileAnswer.ToString() +
                        " and Answers.ImageID = Images.ImageID");
                    Texture2D tileAnsTex = Content.Load<Texture2D>("QuestionAnswerImages/" + filename);

                    //Shift Tile Position
                    if (i != 0)
                    {
                        posRectAns.X += boardWidth / BOARD_TILE_WIDTH;

                        if ((i % BOARD_TILE_WIDTH) == 0)
                        {
                            //posRectAns.X -= BOARD_TILE_WIDTH * (boardWidth / 4);
                            posRectAns.X -= boardWidth;
                            posRectAns.Y += boardWidth / BOARD_TILE_WIDTH;
                        }
                    }

                    BingoTile bt;
                    if (playerIndex < (PLAYER_COUNT / 2))
                    {
                        bt = new BingoTile(Game, tileAnsTex, CorrectSpriteStrip, IncorrectSpriteStrip, posRectAns, posRectAns, (float)Math.PI, new Vector2(tileAnsTex.Width, tileAnsTex.Height));
                    }
                    else
                    {
                        bt = new BingoTile(Game, tileAnsTex, CorrectSpriteStrip, IncorrectSpriteStrip, posRectAns, posRectAns);
                    }

                    bt.Initialize((int)tileAnswer);
                    PlayerTiles[playerIndex].Add(bt);

                    // Store all possible questions for the answer tiles in a question pool to be used by QuestionButton
                    Question.AddQuestions((int)tileAnswer);
                    i++;
                }
            }

            #endregion

            #region Set Question
            Question.RandomiseQuestion();


            // Notify all bingo tiles that a question has been set
            DataTable answerImages = DBhelper.queryDBRows("select ImageID from Answers where QuestionID = " + Question.getID().ToString());
            List<int> list = new List<int>();
            for (int j = 0; j < answerImages.Rows.Count; j++)
            {
                list.Add(Int32.Parse(answerImages.Rows[j].ItemArray[0].ToString()));
            }
            foreach (List<BingoTile> pt in PlayerTiles)
            {
                foreach (BingoTile bt in pt)
                {
                    bt.UpdateQuestion(list);

                }
            }

            Texture2D quAreaTex = Content.Load<Texture2D>("BingoEnvironment/BingoQuestionMark_Area");
            Vector2 originQuArea = new Vector2(quAreaTex.Width / 2, quAreaTex.Height / 2);
            Rectangle posQuArea = new Rectangle(ScreenWidth / 2, ScreenHeight / 2, ScreenWidth, ScreenHeight);
            //Texture, RectDestination, Orientation, imagePositionOrigin
            QuestionArea = new BackgroundItem(quAreaTex, posQuArea, 0, originQuArea);

            #endregion

            #region Player Data

            for (int i = 0; i < PLAYER_COUNT; i++)
            {
                PlayerData[i] = new Player(PlayerTiles[i], i, Game);
            }

            #endregion
            //Question.SelectQuestions(this.Topics);
        }

        public void Draw(GameTime gameTime)
        {
            PlayerColors.Draw(SpriteBatch);

            foreach (BackgroundItem bi in BingoBoards)
            {
                bi.Draw(SpriteBatch);
            }

            foreach (BackgroundItem bi in Dividers)
            {
                bi.Draw(SpriteBatch);
            }

            QuestionArea.Draw(SpriteBatch);
            Question.Draw(SpriteBatch, gameTime);


            for (int playerCount = 0; playerCount < PLAYER_COUNT; playerCount++)
            {
                foreach (BingoTile bt in PlayerTiles[playerCount])
                {
                    bt.Draw(SpriteBatch);
                }
            }

            foreach (Player player in PlayerData)
            {
                player.Draw(SpriteBatch, Game.GraphicsDevice);
            }
        }

        public void Update(GameTime gameTime, ReadOnlyTouchPointCollection touches)
        {
            if (TouchesPrevState == null)
            {
                TouchesPrevState = touches;
            }
                
            foreach (TouchPoint touch in touches)
            {

                //var result = from oldtouch in TouchesPrevState
                //             from newtouch in touches
                //             where Helper.Geometry.Contains(newtouch.Bounds, oldtouch.X, oldtouch.Y) &&
                //             newtouch.Id == oldtouch.Id
                //             select oldtouch;

                //var sameTouch = result.FirstOrDefault();
                //if (sameTouch != null)
                //{
                //    continue;
                //}
                TagData td = touch.Tag;
                if ((td.Value == 0xC0 || td.Value == 8) && !this.QuestionChanged)
                {
                    // Enable question changing
                    Question.Enabled = true;
                    //this.QuestionChanged = true;
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
                    this.QuestionChanged = true;

                    //Notify tiles of new question ID
                    for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                    {
                        foreach (BingoTile bt in PlayerTiles[playerIndex])
                        {
                            // Get possible answer images
                            DataTable dt = DBhelper.queryDBRows("select ImageID from Answers where QuestionID = " + questionID.ToString());
                            List<int> list = new List<int>();
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                list.Add(Int32.Parse(dt.Rows[i].ItemArray[0].ToString()));
                            }

                            bt.UpdateQuestion(list);
                        }
                    }
                }
                //TouchesPrevState = touches;
                //QuestionChanged = false;
            }
            TouchesPrevState = touches;
            QuestionChanged = false;

            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
            {
                foreach (BingoTile bt in PlayerTiles[playerIndex])
                {
                    bt.Update(gameTime);
                }
            }

            if (Question != null)
                Question.Update(gameTime);
        }

        public void Update(GameTime gameTime, MouseState ms)
        {
            //Check for tile clicked
            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
            {
                int tileNum = 0;
                foreach (BingoTile bt in PlayerTiles[playerIndex])
                {
                    bt.ClickEvent(ms);
                    PlayerData[playerIndex].tileAnswered(bt.Answered, tileNum);
                    tileNum++;
                }
            }

            //Check for question clicked
            if (Question.Enabled && Question.OnClickGesture(ms))
            {
                int questionID = Question.getID();

                //Notify tiles of new question ID
                for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
                {
                    // Get possible answer images
                    DataTable dt = DBhelper.queryDBRows("select ImageID from Answers where QuestionID = " + questionID.ToString());

                    foreach (BingoTile bt in PlayerTiles[playerIndex])
                    {
                        List<int> list = new List<int>();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            list.Add(Int32.Parse(dt.Rows[i].ItemArray[0].ToString()));
                        }

                        bt.UpdateQuestion(list);
                    }
                }
            }


            for (int playerIndex = 0; playerIndex < PLAYER_COUNT; playerIndex++)
            {
                foreach (BingoTile bt in PlayerTiles[playerIndex])
                {
                    bt.Update(gameTime);
                }
            }

            if (Question != null)
                Question.Update(gameTime);
        }


        public void UpdateQuestions(int AnswerImage)
        {
            this.Question.RemoveQuestions(AnswerImage);
        }
    }
}
