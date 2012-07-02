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
    class DatabaseHelper
    {
        private SqlCeConnection conn;

        public DatabaseHelper()
        {
            connectDB();
        }

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
    }
}
