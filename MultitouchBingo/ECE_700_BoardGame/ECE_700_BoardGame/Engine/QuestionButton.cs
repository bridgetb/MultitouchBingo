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
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlServerCe;

namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This class represents the centre button containing the question or item called out in each Bingo iteration.
    /// </summary>
    class QuestionButton : Button
    {
        private DataTable questions;
        private string currentTopic {get; set;}
        private string currentQuestion;
        private int questionID;
        private ArrayList completedQuestions;
        private int maxQuestions;
        private ContentManager content;

        public QuestionButton(Game game, Texture2D tex, Rectangle pos, string topic)
            : base(game, tex, pos)
        {
            connectDB();
            questions = SelectQuestions(topic);
            completedQuestions = new ArrayList();
            content = game.Content;

            // Set max questions to ask
            string result = stringQueryDB("select count(*) from Questions");
            maxQuestions = Int32.Parse(result);

            // Set starting question
            currentTopic = topic;
            RandomiseQuestion();
            
        }


        ///// <summary>
        ///// This is called when the touch target receives a tap.
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="args"></param>
        //public bool OnTouchTapGesture(object sender, TouchEventArgs args)
        //{
        //    if (IsPressed(args.TouchPoint))
        //    {
        //        RandomiseQuestion();
        //        return true;
        //    }
        //    return false;
        //}

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
            string topic = currentTopic;
            // If all questions have been cycled through, repeat questions
            if (completedQuestions.Count == maxQuestions)
            {
                completedQuestions.Clear();
#if DEBUG
                Debug.WriteLine("Repeating questions");
#endif
            }

            // Get new question from question set
            int rand = new Random().Next(questions.Rows.Count);
            while (completedQuestions.Contains(rand))
            {
                rand = new Random().Next(questions.Rows.Count);
            }

            object[] row = questions.Rows[rand].ItemArray;
            questionID = Int32.Parse(row[0].ToString());
            currentQuestion = row[1].ToString();

            // Image exists
            string filename = stringQueryDB("select Path from Questions, Images where QuestionID = " + questionID.ToString() + " and Questions.ImageID = Images.ImageID");
                
            // Update image to load as texture
            texture = this.Game.Content.Load<Texture2D>(filename);
                            
            completedQuestions.Add(rand);
            this.Enabled = false;
        }

        /// <summary>
        /// Finds all questions associated with a topic in the database
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>DataTable with each row containing the question ID, question and a boolean to indicate whether there is an image associated with it</returns>
        private DataTable SelectQuestions(string topic)
        {
            
            string query;
            if (topic.Equals("Any"))
            {
                query = "select QuestionID, Question, ImageID from Questions";
            }
            else
            {
                query = "select QuestionID, Question, ImageID from Questions, Topics where Topics.TopicID = Questions.TopicID and Topic = '" + topic + "'";
            }
            DataTable dt = queryDBRows(query);
            
            return dt;
        }

        #region Database Calls
        private SqlCeConnection conn;

        private void connectDB()
        {
            conn = new SqlCeConnection();

            conn.ConnectionString = @"Data Source='|DataDirectory|\ExerciseMaterial.sdf'; File Mode='shared read'";

            conn.Open();
            
        }

        public void disconnectDB()
        {
            conn.Close();
        }

        public DataTable queryDBRows(string query)
        {
            SqlCeCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            try
            {
                cmd.CommandText = query;
                DataTable dt = new DataTable();
                SqlCeDataReader reader = cmd.ExecuteReader();
                int cols = reader.FieldCount;
                for (int i = 0; i < cols; i++)
                {
                    dt.Columns.Add(new DataColumn(reader.GetName(i)));
                }

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < cols; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    dt.Rows.Add(row);

                }
                return dt;
            }
            catch (SqlException ex)
            {
                return null;
            }
        }

        public string stringQueryDB(string query)
        {
            SqlCeCommand cmd = conn.CreateCommand();
            cmd.Connection = conn;
            try
            {
                cmd.CommandText = query;
                object result = cmd.ExecuteScalar();
                
                if (result != null)
                {
                    string r = result.ToString();
                    return r;
                }
                return null;
            }
            catch (SqlException ex)
            {
                return null;
            }
        }
        #endregion

        public int getID()
        {
            return questionID;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            base.Draw(batch);
            float scale = 1;
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
            batch.DrawString(font, part1, new Vector2(position.X + position.Width / 2 - vec.X / 2, position.Y + position.Height), Color.Black,
                0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            batch.DrawString(font, part1, new Vector2(position.X + position.Width / 2 + vec.X / 2, position.Y), Color.Black,
                Single.Parse(Math.PI.ToString()), new Vector2(0,0), scale, SpriteEffects.None, 0);
            batch.DrawString(font, part2, new Vector2(position.X + position.Width / 2 - vec.X / 2, position.Y + position.Height + vec.Y), Color.Black,
                0, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            batch.DrawString(font, part2, new Vector2(position.X + position.Width / 2 + vec.X / 2, position.Y - vec.Y), Color.Black,
                Single.Parse(Math.PI.ToString()), new Vector2(0, 0), scale, SpriteEffects.None, 0);


        }
    }   
}
