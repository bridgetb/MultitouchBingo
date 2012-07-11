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
    class QuestionButton : Button
    {
        private DataTable questions;
        private List<String> currentTopics {get; set;}
        private string currentQuestion;
        private int questionID;
        private List<int> completedQuestions;
        private List<int> possibleQuestions;
        private int maxQuestions;
        private ContentManager content;
        private DatabaseHelper databaseHelper;
        private float Rotation;

        public QuestionButton(Game game, Texture2D tex, Rectangle pos, List<String> topics, List<int> possibleQuestions, DatabaseHelper dbhelper)
            : base(game, tex, pos)
        {
            databaseHelper = dbhelper;
            Rotation = 0;
            
            completedQuestions = new List<int>();
            content = game.Content;

            originOffset = new Vector2(0, 0);

            // Set max questions to ask
            string result = databaseHelper.stringQueryDB("select count(*) from Questions");
            maxQuestions = possibleQuestions.Count;
            this.possibleQuestions = possibleQuestions;

            // Set starting question
            currentTopics = topics;
            SelectQuestions(currentTopics);
            RandomiseQuestion();
        }

        public bool OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch))
            {
                RandomiseQuestion();
                return true;
            }
            return false;
        }

        public bool OnClickGesture(MouseState mouseState)
        {
            if (IsPressed(mouseState))
            {
                RandomiseQuestion();
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
            if (completedQuestions.Count == possibleQuestions.Count)
            {
                completedQuestions.Clear();
            }

            // Get new question from question set
            int rand = new Random().Next(possibleQuestions.Count);
            questionID = possibleQuestions.ElementAt(rand);
            while (completedQuestions.Contains(questionID))
            {
                rand = (rand + 1) % possibleQuestions.Count;
                questionID = possibleQuestions.ElementAt(rand);
            }
            // Get question text from database
            currentQuestion = databaseHelper.stringQueryDB("select Question from Questions where QuestionID = " + questionID.ToString());
            
            // Get question image
            string filename = databaseHelper.stringQueryDB("select Path from Questions, Images where QuestionID = " + questionID.ToString() 
                + " and Questions.ImageID = Images.ImageID");
                
            // Update image to load as texture
            texture = this.Game.Content.Load<Texture2D>("QuestionAnswerImages/"+filename);

            originOffset.X = texture.Width / 2;
            originOffset.Y = texture.Height / 2;

            completedQuestions.Add(questionID);
            this.Enabled = false;
        }

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
                    + getTopicClause();
            }
            DataTable dt = databaseHelper.queryDBRows(query);
            questions = dt;
            return dt;
        }

        public int getID()
        {
            return questionID;
        }

        public string getTopicClause()
        {
            string topicClause = "(Topic = ";
            for (int i = 0; i < currentTopics.Count; i++)
            {
                topicClause += "'" + currentTopics.ElementAt(i) + "'";
                if (i < currentTopics.Count - 1)
                {
                    topicClause += " or Topic = ";
                }
                else
                {
                    topicClause += ")";
                }
            }
            return topicClause;
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
            batch.DrawString(font, part1, new Vector2(position.X - vec.X*scale / 2, position.Y + position.Height/2 + 50), Color.Black,
                0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            batch.DrawString(font, part1, new Vector2(position.X + vec.X * scale / 2, position.Y - position.Height/2 - 50), Color.Black,
                Single.Parse(Math.PI.ToString()), new Vector2(0,0), scale, SpriteEffects.None, 0);
            // Line 2 (if any)
            batch.DrawString(font, part2, new Vector2(position.X - vec.X * scale / 2, position.Y + position.Height/2 + 50 + vec.Y * scale), Color.Black,
                0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            batch.DrawString(font, part2, new Vector2(position.X + vec.X * scale / 2, position.Y - position.Height/2 - 50 - vec.Y * scale), Color.Black,
                Single.Parse(Math.PI.ToString()), new Vector2(0, 0), scale, SpriteEffects.None, 0);


        }
    }   
}
