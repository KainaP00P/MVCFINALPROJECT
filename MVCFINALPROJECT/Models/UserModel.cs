using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the Users table
    public class UserModel
    {
        // Properties match the columns in the Users table
        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }       // "Admin" or "Student"
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // -------------------------------------------------------
        // Get all users from the database
        // -------------------------------------------------------
        public static DataTable GetAllUsers()
        {
            string query = "SELECT * FROM Users ORDER BY CreatedAt DESC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get a single user by their ID
        // -------------------------------------------------------
        public static DataTable GetUserByID(int userID)
        {  
            string query = "SELECT * FROM Users WHERE UserID = @UserID";
            SqlParameter[] param = {
                new SqlParameter("@UserID", userID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get a user by Username (used for login)
        // -------------------------------------------------------
        public static DataTable GetUserByUsername(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @Username";
            SqlParameter[] param = {
                new SqlParameter("@Username", username)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new user (Admin creates student accounts)
        // -------------------------------------------------------
        public static int InsertUser(string username, string passwordHash, string role)
        {
            string query = @"INSERT INTO Users (Username, PasswordHash, Role) 
                             VALUES (@Username, @PasswordHash, @Role)";
            SqlParameter[] param = {
                new SqlParameter("@Username", username),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@Role", role)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Update an existing user
        // -------------------------------------------------------
        public static int UpdateUser(int userID, string username, string role, bool isActive)
        {
            string query = @"UPDATE Users 
                             SET Username = @Username, 
                                 Role = @Role, 
                                 IsActive = @IsActive 
                             WHERE UserID = @UserID";
            SqlParameter[] param = {
                new SqlParameter("@UserID", userID),
                new SqlParameter("@Username", username),
                new SqlParameter("@Role", role),
                new SqlParameter("@IsActive", isActive)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Delete a user by ID
        // -------------------------------------------------------
        public static int DeleteUser(int userID)
        {
            string query = "DELETE FROM Users WHERE UserID = @UserID";
            SqlParameter[] param = {
                new SqlParameter("@UserID", userID)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Check if username already exists (for validation)
        // -------------------------------------------------------
        public static bool UsernameExists(string username)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            SqlParameter[] param = {
                new SqlParameter("@Username", username)
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return count > 0; // returns true if username is taken
        }
    }
} 