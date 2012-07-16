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


namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class BingoTile : Button
    {
        #region Fields

        public bool Answered;

        public bool AttemptAnswer;
        
        int ImageID;
        List<int> AnswersToCurrentQuestion;
        
        Texture2D AnsweredSprite;
        Texture2D ErrorSprite;

        Vector2 ansSpriteOffset;
        Vector2 errSpriteOffset;
        float TileOrient;
        Boolean Rotated;

        #endregion

        public BingoTile(Game game, Texture2D tileSprite, Texture2D daubSprite, Texture2D errorSprite, Rectangle pos)
            : base(game, tileSprite, pos)
        {
            this.Answered = false;
            this.AttemptAnswer = false;
            this.Rotated = false;
            
            this.ImageID = -1;
            this.AnswersToCurrentQuestion = new List<int>();
            
            this.AnsweredSprite = daubSprite;
            this.ErrorSprite = errorSprite;
        }

        public BingoTile(Game game, Texture2D tileSprite, Texture2D daubSprite, Texture2D errorSprite, Rectangle pos, float tileOrientation, Vector2 originOffset)
            : base(game, tileSprite, pos)
        {
            this.Answered = false;
            this.AttemptAnswer = false;

            this.ImageID = -1;
            this.AnswersToCurrentQuestion = new List<int>();

            this.AnsweredSprite = daubSprite;
            this.ErrorSprite = errorSprite;

            this.Rotated = true;
            this.TileOrient = tileOrientation;
            this.originOffset = originOffset;
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
        /// This is called when the touch target receives a tap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnTouchTapGesture(object sender, TouchEventArgs args)
        {
            if (IsPressed(args.TouchPoint) && !this.Answered)
            {
                if (IsCorrectAnswer())
                {
                    this.Answered = true;
                }
                else
                {
                    this.AttemptAnswer = true;
                }
            }
        }

        public void OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch) && !this.Answered)
            {
                if (IsCorrectAnswer())
                {
                    this.Answered = true;
                    // TODO: Call back to BingoApp to remove answer
                }
                else
                {
                    this.AttemptAnswer = true;
                }
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
                    // TODO: Call back to BingoApp to remove answer
                }
                else
                {
                    this.AttemptAnswer = true;
                }
            }
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(List<int> answersToCurrentQuestion)
        {
            this.AnswersToCurrentQuestion = answersToCurrentQuestion;
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
        public void Draw(SpriteBatch spriteBatch)
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
                    spriteBatch.Draw(AnsweredSprite, position, null, Color.White, TileOrient, ansSpriteOffset, SpriteEffects.None, 0f);
                }
                else
                {
                    //base.Draw(spriteBatch);
                    spriteBatch.Draw(AnsweredSprite, position, Color.White);
                }
            }
            else if(AttemptAnswer)
            {
                AttemptAnswer = false;
                if (Rotated)
                {
                    spriteBatch.Draw(ErrorSprite, position, null, Color.White, TileOrient, errSpriteOffset, SpriteEffects.None, 0f);
                }
                else
                {
                    //base.Draw(spriteBatch);
                    spriteBatch.Draw(ErrorSprite, position, Color.White);
                }
            }
        }
    }
}
