using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        public QuestionButton(Game game, Texture2D tex, Rectangle pos, string topic)
            : base(game, tex, pos)
        {
            questions = SelectQuestions(topic);
            
            // Set starting question
            currentTopic = topic;
            RandomiseQuestion();
        }


        /// <summary>
        /// This is called when the touch target receives a tap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void OnTouchTapGesture(object sender, TouchEventArgs args)
        {
            if (IsPressed(args.TouchPoint))
            {
                RandomiseQuestion();
            }
        }

        /// <summary>
        /// Chooses a random question based on the current topic.
        /// </summary>
        public void RandomiseQuestion()
        {
            string topic = currentTopic;
            // Get new question from question set
            int rand = new Random().Next(questions.Rows.Count);
            object[] row = questions.Rows[rand].ItemArray;
            questionID = Int32.Parse(row[0].ToString());
            currentQuestion = row[1].ToString();

            if (Convert.ToBoolean(row[2]))
            {
                // Image exists
                connectDB();
                string filename = stringQueryDB("select Path from Images where QuestionID = " + questionID.ToString());
                
                // Update image to load as texture
                texture = this.Game.Content.Load<Texture2D>(filename);
                
                disconnectDB();
            }
        }

        /// <summary>
        /// Finds all questions associated with a topic in the database
        /// </summary>
        /// <param name="topic"></param>
        /// <returns>DataTable with each row containing the question ID, question and a boolean to indicate whether there is an image associated with it</returns>
        private DataTable SelectQuestions(string topic)
        {
            connectDB();
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
            disconnectDB();
            return dt;
        }

        #region Database Members
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

        private void disconnectDB()
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

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            base.Draw(batch);
        }
    }   
}
