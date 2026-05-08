using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the Courses table
    public class CourseModel
    {
        // Properties match the columns in the Courses table
        public int CourseID { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Department { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // -------------------------------------------------------
        // Get all courses (active and inactive)
        // -------------------------------------------------------
        public static DataTable GetAllCourses()
        {
            string query = "SELECT * FROM Courses ORDER BY CourseName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get only active courses (used in dropdowns)
        // -------------------------------------------------------
        public static DataTable GetActiveCourses()
        {
            string query = "SELECT * FROM Courses WHERE IsActive = 1 ORDER BY CourseName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get a single course by CourseID
        // -------------------------------------------------------
        public static DataTable GetCourseByID(int courseID)
        {
            string query = "SELECT * FROM Courses WHERE CourseID = @CourseID";
            SqlParameter[] param = {
                new SqlParameter("@CourseID", courseID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new course
        // -------------------------------------------------------
        public static int InsertCourse(CourseModel c)
        {
            string query = @"INSERT INTO Courses 
                            (CourseCode, CourseName, Department, IsActive)
                             VALUES 
                            (@CourseCode, @CourseName, @Department, @IsActive)";

            SqlParameter[] param = {
                new SqlParameter("@CourseCode", c.CourseCode),
                new SqlParameter("@CourseName", c.CourseName),
                new SqlParameter("@Department", (object)c.Department ?? DBNull.Value),
                new SqlParameter("@IsActive", c.IsActive)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Update an existing course
        // -------------------------------------------------------
        public static int UpdateCourse(CourseModel c)
        {
            string query = @"UPDATE Courses SET
                             CourseCode = @CourseCode,
                             CourseName = @CourseName,
                             Department = @Department,
                             IsActive = @IsActive
                             WHERE CourseID = @CourseID";

            SqlParameter[] param = {
                new SqlParameter("@CourseID", c.CourseID),
                new SqlParameter("@CourseCode", c.CourseCode),
                new SqlParameter("@CourseName", c.CourseName),
                new SqlParameter("@Department", (object)c.Department ?? DBNull.Value),
                new SqlParameter("@IsActive", c.IsActive)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Delete a course by ID
        // -------------------------------------------------------
        public static int DeleteCourse(int courseID)
        {
            string query = "DELETE FROM Courses WHERE CourseID = @CourseID";
            SqlParameter[] param = {
                new SqlParameter("@CourseID", courseID)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Check if CourseCode already exists (for validation)
        // -------------------------------------------------------
        public static bool CourseCodeExists(string courseCode, int excludeID = 0)
        {
            // excludeID is used during UPDATE to ignore the current record
            string query = @"SELECT COUNT(*) FROM Courses 
                             WHERE CourseCode = @CourseCode 
                             AND CourseID != @ExcludeID";
            SqlParameter[] param = {
                new SqlParameter("@CourseCode", courseCode),
                new SqlParameter("@ExcludeID", excludeID)
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return count > 0;
        }
    }
}