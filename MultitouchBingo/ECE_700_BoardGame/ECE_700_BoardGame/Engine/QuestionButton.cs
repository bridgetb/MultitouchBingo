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

        public QuestionButton(Game game, Texture2D tex, Rectangle pos, string topic)
            : base(game, tex, pos)
        {
            connectDB();
            questions = SelectQuestions(topic);
            completedQuestions = new ArrayList();

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

            if (Convert.ToBoolean(row[2]))
            {
                // Image exists
                string filename = stringQueryDB("select Path from Images where QuestionID = " + questionID.ToString());
                
                // Update image to load as texture
                texture = this.Game.Content.Load<Texture2D>(filename);
                
            }
            completedQuestions.Add(rand);
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
                query = "select QuestionID, Question, HasImage from Questions";
            }
            else
            {
                query = "select QuestionID, Question, HasImage from Questions, Topics where Topics.TopicID = Questions.TopicID and Topic = '" + topic + "'";
            }
            DataTable dt = queryDBRows(query);
            
            return dt;
        }

        #region Database Calls
        private SqlCeConnection conn;

        private void connectDB()
        {
            conn = new SqlCeConnection();
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            DirectoryInfo d = Directory.GetParent(a.CodeBase.ToString().Substring(8));
            while (!d.ToString().EndsWith(@"\ECE_700_BoardGame"))
            {
                d = Directory.GetParent(d.ToString());
            }
            String dbfile = d.ToString() + @"\ExerciseMaterial.sdf";
            conn.ConnectionString = @"Data Source='ExerciseMaterial.sdf'; File Mode='shared read'";

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
        }
    }   
}
