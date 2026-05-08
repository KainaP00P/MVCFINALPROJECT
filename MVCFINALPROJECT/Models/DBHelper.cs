using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace MVCFINALPROJECT.Models
{
    // DBHelper handles all database connection and query operations
    // All controllers will use this class to talk to the database
    public class DBHelper
    {
        // Get connection string from Web.config
        private static string connString =
            ConfigurationManager.ConnectionStrings["StudentAccDB"].ConnectionString;

        // Returns an open SqlConnection
        public static SqlConnection GetConnection()
        {
            SqlConnection conn = new SqlConnection(connString);
            conn.Open();
            return conn;
        }

        // Executes INSERT, UPDATE, DELETE queries
        // Returns number of rows affected
        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // Add parameters if provided (prevents SQL injection)
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteNonQuery();
            }
        }

        // Executes SELECT queries
        // Returns a DataTable filled with results
        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                DataTable dt = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
                return dt;
            }
        }

        // Executes a query that returns a single value (e.g. COUNT, MAX, or a single field)
        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteScalar();
            }
        }
    }
}