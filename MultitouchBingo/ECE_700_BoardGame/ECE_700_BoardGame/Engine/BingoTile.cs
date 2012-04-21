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


namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class BingoTile : Button
    {
        #region Fields

        bool Answered;
        bool AttemptAnswer;
        
        int AnswerID;
        int QuestionID;
        
        Texture2D AnsweredSprite;
        Texture2D ErrorSprite;

        #endregion

        public BingoTile(Game game, Texture2D tileSprite, Texture2D daubSprite, Texture2D errorSprite, Rectangle pos)
            : base(game, tileSprite, pos)
        {
            this.Answered = false;
            this.AttemptAnswer = false;
            
            this.AnswerID = -1;
            this.QuestionID = -1;
            
            this.AnsweredSprite = daubSprite;
            this.ErrorSprite = errorSprite;
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(int ansId)
        {
            this.AnswerID = ansId;
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


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(int questionId)
        {
            this.QuestionID = questionId;
        }

        private bool IsCorrectAnswer()
        {
            try
            {
                if ((AnswerID < 0) || (QuestionID < 0))
                    throw new System.ArgumentException("ID cannot be less than 0");

                if ((AnswerID == QuestionID) && (AnswerID >= 0) && (QuestionID >= 0))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Answer or Question ID Not initialized for tile");
                Debug.WriteLine(AnswerID.ToString(), "AnswerID");
                Debug.WriteLine(QuestionID.ToString(), "QuestionID");
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
            if (!Answered && !AttemptAnswer)
            {
                base.Draw(spriteBatch);
            }
            else if (Answered)
            {
                base.Draw(spriteBatch);
                spriteBatch.Draw(AnsweredSprite, position, Color.White);
            }
            else
            {
                AttemptAnswer = false;
                spriteBatch.Draw(ErrorSprite, position, Color.White);
                base.Draw(spriteBatch);
            }
        }
    }
}
