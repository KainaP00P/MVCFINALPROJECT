using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the Scholarships table
    public class ScholarshipModel
    {
        // Properties match the columns in the Scholarships table
        public int ScholarshipID { get; set; }
        public string ScholarshipName { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; } // e.g. 100 = full, 50 = half
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // -------------------------------------------------------
        // Get all scholarships (active and inactive)
        // -------------------------------------------------------
        public static DataTable GetAllScholarships()
        {
            string query = "SELECT * FROM Scholarships ORDER BY ScholarshipName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get only active scholarships (used in dropdowns)
        // -------------------------------------------------------
        public static DataTable GetActiveScholarships()
        {
            string query = @"SELECT * FROM Scholarships 
                             WHERE IsActive = 1 
                             ORDER BY ScholarshipName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get a single scholarship by ID
        // -------------------------------------------------------
        public static DataTable GetScholarshipByID(int scholarshipID)
        {
            string query = "SELECT * FROM Scholarships WHERE ScholarshipID = @ScholarshipID";
            SqlParameter[] param = {
                new SqlParameter("@ScholarshipID", scholarshipID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new scholarship
        // -------------------------------------------------------
        public static int InsertScholarship(ScholarshipModel s)
        {
            string query = @"INSERT INTO Scholarships
                            (ScholarshipName, Description, DiscountPercent, IsActive)
                             VALUES
                            (@ScholarshipName, @Description, @DiscountPercent, @IsActive)";

            SqlParameter[] param = {
                new SqlParameter("@ScholarshipName", s.ScholarshipName),
                new SqlParameter("@Description", (object)s.Description ?? DBNull.Value),
                new SqlParameter("@DiscountPercent", s.DiscountPercent),
                new SqlParameter("@IsActive", s.IsActive)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Update an existing scholarship
        // -------------------------------------------------------
        public static int UpdateScholarship(ScholarshipModel s)
        {
            string query = @"UPDATE Scholarships SET
                             ScholarshipName = @ScholarshipName,
                             Description = @Description,
                             DiscountPercent = @DiscountPercent,
                             IsActive = @IsActive
                             WHERE ScholarshipID = @ScholarshipID";

            SqlParameter[] param = {
                new SqlParameter("@ScholarshipID", s.ScholarshipID),
                new SqlParameter("@ScholarshipName", s.ScholarshipName),
                new SqlParameter("@Description", (object)s.Description ?? DBNull.Value),
                new SqlParameter("@DiscountPercent", s.DiscountPercent),
                new SqlParameter("@IsActive", s.IsActive)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Delete a scholarship by ID
        // -------------------------------------------------------
        public static int DeleteScholarship(int scholarshipID)
        {
            string query = "DELETE FROM Scholarships WHERE ScholarshipID = @ScholarshipID";
            SqlParameter[] param = {
                new SqlParameter("@ScholarshipID", scholarshipID)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Check if scholarship name already exists (for validation)
        // -------------------------------------------------------
        public static bool ScholarshipNameExists(string name, int excludeID = 0)
        {
            // excludeID is used during UPDATE to ignore the current record
            string query = @"SELECT COUNT(*) FROM Scholarships
                             WHERE ScholarshipName = @ScholarshipName
                             AND ScholarshipID != @ExcludeID";
            SqlParameter[] param = {
                new SqlParameter("@ScholarshipName", name),
                new SqlParameter("@ExcludeID", excludeID)
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return count > 0;
        }
    }
}