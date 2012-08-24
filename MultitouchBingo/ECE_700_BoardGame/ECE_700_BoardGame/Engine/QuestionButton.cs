using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Surface;
using Microsoft.Surface.Core;

using System.Collections;
using System.IO;
using ECE_700_BoardGame.Helper;
using System.Data;

namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This class represents the centre button containing the question or item called out in each Bingo iteration.
    /// </summary>
    class QuestionButton : MenuButton
    {
        private DataTable questions;
        private List<String> currentTopics {get; set;}
        private string currentQuestion;
        private int questionID;
        private List<int> completedQuestions;
        private List<int> PossibleQuestions;
        private Hashtable QuestionFrequency;
        private ContentManager content;
        private DatabaseHelper databaseHelper;
        private float Rotation;

        public QuestionButton(Game game, Texture2D tex, Rectangle pos, Rectangle target, List<String> topics, DatabaseHelper dbhelper)
            : base(game, tex, pos, target)
        {
            databaseHelper = dbhelper;
            Rotation = 0;
            
            completedQuestions = new List<int>();
            content = game.Content;

            OriginOffset = new Vector2(0, 0);
            PossibleQuestions = new List<int>();
            QuestionFrequency = new Hashtable();

            // Set starting question
            currentTopics = topics;
        }

        override public bool OnTouchTapGesture(TouchPoint touch)
        {
            TagData td = touch.Tag;
            if (IsPressed(touch) && (td.Value == 0xC0 || td.Value == 8 || td.Value == 9 || td.Value == 0x0B || td.Value == 0x0A))
            {
                RandomiseQuestion();
                return true;
            }
            return false;
        }

        override public bool OnClickGesture(MouseState mouseState)
        {
            if (IsPressed(mouseState)) // No tags in debug mode - mouse cannot simulate a tag
            {
                RandomiseQuestion();
                return true;
            }
            return false;
        }

        protected override bool IsPressed(TouchPoint point)
        {
            Rectangle largerArea = new Rectangle(Position.X - Position.Height / 2 - 20, Position.Y - Position.Height / 2 - 20, Position.Width + 20, Position.Height + 20);  
#if DEBUG
            Debug.WriteLine(point.X.ToString(), "Touch point X");
            Debug.WriteLine(point.Y.ToString(), "Touch point Y");
            Debug.WriteLine(largerArea.Contains((int)point.X, (int)point.Y).ToString(), "Is Within Item Hit Detection");
#endif
            if (largerArea.Contains((int)point.X, (int)point.Y))
            {
                return true;
            }
            return false;
        }

        protected override bool IsPressed(MouseState clickPoint)
        {
            Rectangle largerArea = new Rectangle(Position.X - Position.Width / 2 - 20, Position.Y - Position.Height / 2 - 20, Position.Width + 20, Position.Height + 20);            
#if DEBUG
            Debug.WriteLine(largerArea.Contains((int)clickPoint.X, (int)clickPoint.Y).ToString(), "Is Within Item Hit Detection (CLICK)");
#endif
            if (largerArea.Contains((int)clickPoint.X, (int)clickPoint.Y))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Chooses a random question based on the current topic.
        /// </summary>
        public void RandomiseQuestion()
        {
            // If all questions have been cycled through, repeat questions
            if (completedQuestions.Distinct().Count() == PossibleQuestions.Count)
            {
                completedQuestions.Clear();
            }

            // Check that game has not finished (i.e. no more questions to ask)
            if (PossibleQuestions.Count == 0)
            {
                // Don't change the question
                return;
            }

            // Get new question from question set
            int rand = new Random().Next(PossibleQuestions.Count);
            questionID = PossibleQuestions.ElementAt(rand);
            //while (completedQuestions.Contains(questionID))
            //{
            //    rand = (rand + 1) % PossibleQuestions.Count;
            //    questionID = PossibleQuestions.ElementAt(rand);
            //}
            // Get question text from database
            currentQuestion = databaseHelper.stringQueryDB("select Question from Questions where QuestionID = " + questionID.ToString());
            
            // Get question image
            string filename = databaseHelper.stringQueryDB("select Path from Questions, Images where QuestionID = " + questionID.ToString() 
                + " and Questions.ImageID = Images.ImageID");
                
            // Update image to load as texture
            Texture = this.Game.Content.Load<Texture2D>("QuestionAnswerImages/"+filename);

            OriginOffset.X = Texture.Width / 2;
            OriginOffset.Y = Texture.Height / 2;

            completedQuestions.Add(questionID);
        }
        /*
        /// <summary>
        /// Finds all questions associated with a topic in the database
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>DataTable with each row containing the question ID, question and a boolean to indicate whether there is an image associated with it</returns>
        public DataTable SelectQuestions(List<String> topic)
        {
            currentTopics = topic;
            string query;
            if (topic.Count == 3 || topic.Count == 0)
            {
                query = "select QuestionID, Question, ImageID from Questions";
            }
            else
            {                
                query = "select QuestionID, Question, ImageID from Questions, Topics where Topics.TopicID = Questions.TopicID and " 
                    + databaseHelper.getQueryClause("Topic", currentTopics);
            }
            DataTable dt = databaseHelper.queryDBRows(query);
            questions = dt;
            return dt;
        }*/

        public int getID()
        {
            return questionID;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public new void Update(GameTime gameTime)
        {
            Rotation += 0.005f;
            Rotation = (Rotation >= (Math.PI * 2)) ? 0 : Rotation;
        }

        public void RemoveQuestions(int answerImage)
        {
            // Remove questions to answer image from pool
            DataTable questionIds = databaseHelper.queryDBRows(
                        "select distinct Questions.QuestionID from Questions, Answers where Questions.QuestionID = Answers.QuestionID and Answers.ImageID = "
                        + answerImage.ToString());
            for (int j = 0; j < questionIds.Rows.Count; j++)
            {
                Int32 qId = Int32.Parse(questionIds.Rows[j].ItemArray[0].ToString());
                Int32 freq = Int32.Parse(this.QuestionFrequency[qId].ToString());
                QuestionFrequency.Remove(qId);
                if (freq > 1) // decrement frequency if greater than 1, otherwise remove from hashtable
                {
                    freq--;
                    this.QuestionFrequency.Add(qId, freq);
                }
                else
                { // Remove question from list
                    while (this.PossibleQuestions.Remove(qId))
                    {
                        Debug.WriteLine("Removed");
                    }
                }
            }
        }

        public void AddQuestions(int answerImage)
        {
            // Add potential questions to answer image from pool
            DataTable questionIds = databaseHelper.queryDBRows(
                        "select distinct Questions.QuestionID from Questions, Answers where Questions.QuestionID = Answers.QuestionID and Answers.ImageID = "
                        + answerImage.ToString());
            for (int j = 0; j < questionIds.Rows.Count; j++)
            {
                Int32 qId = Int32.Parse(questionIds.Rows[j].ItemArray[0].ToString());
                Int32 freq = 1;
                if (QuestionFrequency.Contains(qId))
                {
                    freq = Int32.Parse(QuestionFrequency[qId].ToString());
                    freq++;
                    QuestionFrequency.Remove(qId);
                } else 
                {
                    PossibleQuestions.Add(qId);
                }
                QuestionFrequency.Add(qId, freq);
                    
            }
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            base.Draw(batch, Rotation);
            float scale = 1;
#if DEBUG
            scale = 0.8f;
#endif
            SpriteFont font = content.Load<SpriteFont>("Comic");

            Vector2 vec = font.MeasureString(this.currentQuestion);
            String part1 = this.currentQuestion;
            String part2 = "";
            if (part1.IndexOf("\\n") != -1)
            {
                part1 = this.currentQuestion.Substring(0, part1.IndexOf("\\n"));
                vec = font.MeasureString(part1);
                part2 = this.currentQuestion.Substring(this.currentQuestion.IndexOf("\\n") + 2, this.currentQuestion.Length - this.currentQuestion.IndexOf("\\n") -2);
            }
            
            
            // Line 1
            batch.DrawString(font, part1, new Vector2(Position.X - vec.X*scale / 2, Position.Y + Position.Height/2 + 50), Color.Black,
                0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            batch.DrawString(font, part1, new Vector2(Position.X + vec.X * scale / 2, Position.Y - Position.Height/2 - 50), Color.Black,
                Single.Parse(Math.PI.ToString()), new Vector2(0,0), scale, SpriteEffects.None, 0);
            // Line 2 (if any)
            batch.DrawString(font, part2, new Vector2(Position.X - vec.X * scale / 2, Position.Y + Position.Height/2 + 50 + vec.Y * scale), Color.Black,
                0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            batch.DrawString(font, part2, new Vector2(Position.X + vec.X * scale / 2, Position.Y - Position.Height/2 - 50 - vec.Y * scale), Color.Black,
                Single.Parse(Math.PI.ToString()), new Vector2(0, 0), scale, SpriteEffects.None, 0);


        }
    }   
}
