using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data.Sql;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlServerCe;

namespace ECE_700_BoardGame.Helper
{
    /// <summary>
    /// Singleton class used for any communication with the SQL Server CE database "ExerciseMaterial.sdf".
    /// </summary>
    class DatabaseHelper
    {
        private static DatabaseHelper instance;
        private SqlCeConnection conn;

        private DatabaseHelper() {
            connectDB();
        }

        public static DatabaseHelper Instance
        {
            get 
            {
                if (instance == null)
                {
                    instance = new DatabaseHelper();
                }
                return instance;
            }
        }

        /// <summary>
        /// Connects to the database named "ExerciseMaterial" containing question and answer material for the game.
        /// </summary>
        private void connectDB()
        {
            conn = new SqlCeConnection();
            conn.ConnectionString = @"Data Source='|DataDirectory|\ExerciseMaterial.sdf'; File Mode='shared read'";
            conn.Open();
        }

        /// <summary>
        /// Closes connection to the database.
        /// </summary>
        public void disconnectDB()
        {
            conn.Close();
        }

        /// <summary>
        /// Queries database for multiple-column results.
        /// </summary>
        /// <param name="query">SQL query in string form</param>
        /// <returns>DataTable containing results of query</returns>
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

        /// <summary>
        /// Queries database for a string value.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Helps with combining query constraints (i.e. the WHERE clause).
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="constraints"></param>
        /// <returns></returns>
        public string getQueryClause(String attribute, List<String> constraints)
        {
            string clause = "(" + attribute + " = ";
            for (int i = 0; i < constraints.Count; i++)
            {
                clause += "'" + constraints.ElementAt(i) + "'";
                if (i < constraints.Count - 1)
                {
                    clause += " or " + attribute + " = ";
                }
                else
                {
                    clause += ") ";
                }
            }
            return clause;
        }
    }
}
