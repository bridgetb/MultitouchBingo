using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Surface.Core;
using System.Diagnostics;
using System.Collections;
using ECE_700_BoardGame.Screens;


namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class BingoTile : Button
    {
        #region Fields


        private const int CORRECT_IMAGE_W = 136;
        private const int CORRECT_IMAGE_H = 136;

        private const int INCORRECT_IMAGE_W = 200;
        private const int INCORRECT_IMAGE_H = 200;

        public bool Answered;
        public bool AttemptAnswer;
        bool Locked;

        bool InWinningRow = false;
        Texture2D Highlight;

        Animation CorrectAnswer;
        Animation IncorrectAnswer;

        int ImageID;
        List<int> AnswersToCurrentQuestion;

        Texture2D AnsweredSprite;
        Texture2D ErrorSprite;

        Vector2 ansSpriteOffset;
        Vector2 errSpriteOffset;
        float TileOrient;
        Boolean Rotated;
        #endregion

        public BingoTile(Game game, Texture2D tileSprite, Texture2D daubSprite, Texture2D errorSprite, Rectangle pos, Rectangle target)
            : base(game, tileSprite, pos, target)
        {
            this.Answered = false;
            this.AttemptAnswer = false;
            this.Rotated = false;
            this.Locked = false;

            this.ImageID = -1;
            this.AnswersToCurrentQuestion = new List<int>();

            this.AnsweredSprite = daubSprite;
            this.ErrorSprite = errorSprite;

            CorrectAnswer = new Animation();
            CorrectAnswer.Initialize(daubSprite, new Vector2(pos.X, pos.Y), new Vector2(pos.X, pos.Y), CORRECT_IMAGE_W, CORRECT_IMAGE_H, 15, 50, Color.White, (float)pos.Width / CORRECT_IMAGE_W, false, true);
            CorrectAnswer.Active = false;

            IncorrectAnswer = new Animation();
            IncorrectAnswer.Initialize(errorSprite, new Vector2(pos.X, pos.Y), new Vector2(pos.X, pos.Y), INCORRECT_IMAGE_W, INCORRECT_IMAGE_H, 7, 50, Color.White, (float)pos.Width / INCORRECT_IMAGE_W, false, false);
            IncorrectAnswer.Active = false;
        }

        public BingoTile(Game game, Texture2D tileSprite, Texture2D daubSprite, Texture2D errorSprite, Rectangle pos, 
            Rectangle target, float tileOrientation, Vector2 originOffset)
            : this(game, tileSprite, daubSprite, errorSprite, pos, target)
        {
            this.Rotated = true;
            this.TileOrient = tileOrientation;
            this.OriginOffset = originOffset;
            this.ansSpriteOffset = new Vector2(daubSprite.Width, daubSprite.Height);
            this.errSpriteOffset = new Vector2(errorSprite.Width, errorSprite.Height);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(int ansImgId)
        {
            this.ImageID = ansImgId;
        }

        /// <summary>
        /// Sets the tile as a winning tile
        /// </summary>
        /// <param name="highlight"></param>
        /// <returns>bool NewlySet: true if the tile was set to a winning tile in this call</returns>
        public bool SetWinningRow(Texture2D highlight)
        {
            if (this.InWinningRow)
            {
                return false;
            }
            else
            {
                this.InWinningRow = true;
                Highlight = highlight;
                return true;
            }
        }
        
        public void OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch) && !this.Answered && !this.Locked)
            {
                if (IsCorrectAnswer())
                {
                    this.Answered = true;
                    CorrectAnswer.Active = true;

                    // Call back to BingoApp to remove answer
                    if (Game is BingoApp)
                    {
                        Screen s = ((BingoApp)Game).GetGameState();
                        if (s is GameScreen)
                            ((GameScreen)s).UpdateQuestions(this.ImageID);
                    }
                }
                else
                {
                    this.AttemptAnswer = true;
                    IncorrectAnswer.Active = true;
                }
            }
        }

        public void OnTouchReveal(TouchPoint touch)
        {
            if (IsPressed(touch) && this.Answered)
            {
                CorrectAnswer.ClearAnim = true;
            }
        }

        public void ClickEvent(MouseState mouseState)
        {
            //Debug.WriteLine("ENTERS CLICKEVENT");
            if (IsPressed(mouseState) && !this.Answered)
            {
                if (IsCorrectAnswer())
                {
                    this.Answered = true;
                    CorrectAnswer.Active = true;

                    // Call back to BingoApp to remove answer
                    if (Game is BingoApp)
                    {
                        Screen s = ((BingoApp)Game).GetGameState();
                        if (s is GameScreen)
                            ((GameScreen)s).UpdateQuestions(this.ImageID);
                    }
                }
                else
                {
                    this.AttemptAnswer = true;
                    IncorrectAnswer.Active = true;
                }
            }
            else if (IsPressed(mouseState) && this.Answered)
            {
                CorrectAnswer.ClearAnim = true;
            }
        }

        public void OnClickReveal(MouseState mouseState)
        {
            if (IsPressed(mouseState) && this.Answered)
            {
                CorrectAnswer.ClearAnim = true;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void UpdateQuestion(List<int> answersToCurrentQuestion)
        {
            this.AnswersToCurrentQuestion = answersToCurrentQuestion;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        override public void Update(GameTime gametime)
        {
            if (CorrectAnswer.Active)
            {
                CorrectAnswer.Update(gametime);
            }
            if (IncorrectAnswer.Active)
            {
                IncorrectAnswer.Update(gametime);
            }
        }

        private bool IsCorrectAnswer()
        {
            try
            {
                if ((ImageID < 0))
                    throw new System.ArgumentException("ID cannot be less than 0");
                foreach (Int64 obj in AnswersToCurrentQuestion)
                {
                    if ((ImageID == obj) && (ImageID >= 0) && (obj >= 0))
                    {
                        return true;
                    }
                }

                // Check for other possible questions with the same answer

                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Answer or Question ID Not initialized for tile");
                Debug.WriteLine(ImageID.ToString(), "AnswerID");
                Debug.WriteLine(AnswersToCurrentQuestion.ToString(), "QuestionID");
                Debug.WriteLine(e.StackTrace.ToString());
                return false;
            }
        }

        /// <summary>
        /// Rendering of the Bingo Tiles that are placed on each players board
        /// </summary>
        /// <param name="spriteBatch"></param>
        override public void Draw(SpriteBatch spriteBatch)
        {
            //if (!Answered && !AttemptAnswer)
            //{
            if (Rotated)
            {
                base.Draw(spriteBatch, (float)Math.PI);
            }
            else
            {
                base.Draw(spriteBatch);
            }
            //}
            if (Answered)
            {
                if (Rotated)
                {
                    CorrectAnswer.Draw(spriteBatch, true);
                    if (CorrectAnswer.ClearAnim)
                    {
                        //spriteBatch.Draw(AnsweredSprite, position, null, Color.White, TileOrient, ansSpriteOffset, SpriteEffects.None, 0f
                        CorrectAnswer.ClearAnim = false;
                    }
                }
                else
                {
                    CorrectAnswer.Draw(spriteBatch, false);
                    if (CorrectAnswer.ClearAnim)
                    {
                        //base.Draw(spriteBatch);
                        //spriteBatch.Draw(AnsweredSprite, position, Color.White);
                        CorrectAnswer.ClearAnim = false;
                    }
                }
            }
            else if (AttemptAnswer)
            {
                AttemptAnswer = false;
                if (Rotated)
                {
                    IncorrectAnswer.Draw(spriteBatch, true);
                    //spriteBatch.Draw(ErrorSprite, position, null, Color.White, TileOrient, errSpriteOffset, SpriteEffects.None, 0f);
                }
                else
                {
                    IncorrectAnswer.Draw(spriteBatch, false);
                    //base.Draw(spriteBatch);
                    //spriteBatch.Draw(ErrorSprite, position, Color.White);
                }
            }
            if (IncorrectAnswer.Active)
            {
                if (Rotated)
                {
                    IncorrectAnswer.Draw(spriteBatch, true);
                }
                else
                {
                    IncorrectAnswer.Draw(spriteBatch, false);
                }
            }

            if (this.InWinningRow)
            {
                // Highlight cell
                //spriteBatch.Draw(Highlight, new Rectangle((int)(this.Position.X - this.Position.Width * 0.25), (int)(this.Position.Y - this.Position.Width * 0.25), 
                //    (int)(this.Position.Width * 1.5), (int)(this.Position.Height * 1.5)), Color.White);
                spriteBatch.Draw(Highlight, new Rectangle((int)(this.Position.X - this.Position.Width / 5), (int)(this.Position.Y - this.Position.Width / 5),
                    (int)(this.Position.Width * 1.5), (int)(this.Position.Height * 1.5)), Color.White);
            }
        }
    }
}
