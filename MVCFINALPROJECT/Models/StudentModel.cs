using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the Students table
    public class StudentModel
    {
        // Properties match the columns in the Students table
        public int StudentID { get; set; }
        public int UserID { get; set; }
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public int CourseID { get; set; }
        public int YearLevel { get; set; }
        public string Semester { get; set; }
        public string SchoolYear { get; set; }
        public string EnrollmentStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        // -------------------------------------------------------
        // Get all students with their Course name joined
        // -------------------------------------------------------
        public static DataTable GetAllStudents()
        {
            string query = @"SELECT s.*, c.CourseName 
                             FROM Students s
                             JOIN Courses c ON s.CourseID = c.CourseID
                             ORDER BY s.LastName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get a single student by StudentID
        // -------------------------------------------------------
        public static DataTable GetStudentByID(int studentID)
        {
            string query = @"SELECT s.*, c.CourseName 
                             FROM Students s
                             JOIN Courses c ON s.CourseID = c.CourseID
                             WHERE s.StudentID = @StudentID";
            SqlParameter[] param = {
                new SqlParameter("@StudentID", studentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get student by UserID (used after login to get profile)
        // -------------------------------------------------------
        public static DataTable GetStudentByUserID(int userID)
        {
            string query = @"SELECT s.*, c.CourseName 
                             FROM Students s
                             JOIN Courses c ON s.CourseID = c.CourseID
                             WHERE s.UserID = @UserID";
            SqlParameter[] param = {
                new SqlParameter("@UserID", userID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new student (Admin only)
        // -------------------------------------------------------
        public static int InsertStudent(StudentModel s)
        {
            string query = @"INSERT INTO Students 
                            (UserID, StudentNumber, FirstName, MiddleName, LastName,
                             Gender, BirthDate, Address, ContactNumber, Email,
                             CourseID, YearLevel, Semester, SchoolYear, EnrollmentStatus)
                             VALUES 
                            (@UserID, @StudentNumber, @FirstName, @MiddleName, @LastName,
                             @Gender, @BirthDate, @Address, @ContactNumber, @Email,
                             @CourseID, @YearLevel, @Semester, @SchoolYear, @EnrollmentStatus)";

            SqlParameter[] param = {
                new SqlParameter("@UserID", s.UserID),
                new SqlParameter("@StudentNumber", s.StudentNumber),
                new SqlParameter("@FirstName", s.FirstName),
                new SqlParameter("@MiddleName", (object)s.MiddleName ?? DBNull.Value),
                new SqlParameter("@LastName", s.LastName),
                new SqlParameter("@Gender", s.Gender),
                new SqlParameter("@BirthDate", s.BirthDate),
                new SqlParameter("@Address", s.Address),
                new SqlParameter("@ContactNumber", s.ContactNumber),
                new SqlParameter("@Email", s.Email),
                new SqlParameter("@CourseID", s.CourseID),
                new SqlParameter("@YearLevel", s.YearLevel),
                new SqlParameter("@Semester", s.Semester),
                new SqlParameter("@SchoolYear", s.SchoolYear),
                new SqlParameter("@EnrollmentStatus", s.EnrollmentStatus)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Update an existing student record
        // -------------------------------------------------------
        public static int UpdateStudent(StudentModel s)
        {
            string query = @"UPDATE Students SET
                             FirstName = @FirstName,
                             MiddleName = @MiddleName,
                             LastName = @LastName,
                             Gender = @Gender,
                             BirthDate = @BirthDate,
                             Address = @Address,
                             ContactNumber = @ContactNumber,
                             Email = @Email,
                             CourseID = @CourseID,
                             YearLevel = @YearLevel,
                             Semester = @Semester,
                             SchoolYear = @SchoolYear,
                             EnrollmentStatus = @EnrollmentStatus
                             WHERE StudentID = @StudentID";

            SqlParameter[] param = {
                new SqlParameter("@StudentID", s.StudentID),
                new SqlParameter("@FirstName", s.FirstName),
                new SqlParameter("@MiddleName", (object)s.MiddleName ?? DBNull.Value),
                new SqlParameter("@LastName", s.LastName),
                new SqlParameter("@Gender", s.Gender),
                new SqlParameter("@BirthDate", s.BirthDate),
                new SqlParameter("@Address", s.Address),
                new SqlParameter("@ContactNumber", s.ContactNumber),
                new SqlParameter("@Email", s.Email),
                new SqlParameter("@CourseID", s.CourseID),
                new SqlParameter("@YearLevel", s.YearLevel),
                new SqlParameter("@Semester", s.Semester),
                new SqlParameter("@SchoolYear", s.SchoolYear),
                new SqlParameter("@EnrollmentStatus", s.EnrollmentStatus)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Delete a student by ID
        // -------------------------------------------------------
        public static int DeleteStudent(int studentID)
        {
            string query = "DELETE FROM Students WHERE StudentID = @StudentID";
            SqlParameter[] param = {
                new SqlParameter("@StudentID", studentID)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Check if StudentNumber already exists (for validation)
        // -------------------------------------------------------
        public static bool StudentNumberExists(string studentNumber)
        {
            string query = "SELECT COUNT(*) FROM Students WHERE StudentNumber = @StudentNumber";
            SqlParameter[] param = {
                new SqlParameter("@StudentNumber", studentNumber)
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return count > 0;
        }

        // -------------------------------------------------------
        // Generate next Student Number automatically
        // Format: YYYY-XXXXX (e.g. 2024-00001)
        // -------------------------------------------------------
        public static string GenerateStudentNumber()
        {
            string year = DateTime.Now.Year.ToString();

            // Get the MAX student number for this year to avoid duplicates after deletes
            // This ensures the number always increments even after deletions
            string query = @"SELECT ISNULL(MAX(CAST(SUBSTRING(StudentNumber, 6, 5) 
                     AS INT)), 0) 
                     FROM Students 
                     WHERE StudentNumber LIKE @Year";
            SqlParameter[] param = {
        new SqlParameter("@Year", year + "-%")
    };
            int maxNumber = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return year + "-" + (maxNumber + 1).ToString("D5");
        }
    }
}