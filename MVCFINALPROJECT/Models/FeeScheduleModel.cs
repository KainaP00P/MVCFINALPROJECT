using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the FeeSchedules table
    // Fee schedule varies per Course per School Year per Semester
    public class FeeScheduleModel
    {
        // Properties match the columns in the FeeSchedules table
        public int FeeScheduleID { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }    // joined from Courses table
        public string SchoolYear { get; set; }    // e.g. "2024-2025"
        public string Semester { get; set; }      // "1st" or "2nd"
        public decimal TuitionFeePerUnit { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal MiscellaneousFee { get; set; }
        public decimal LaboratoryFee { get; set; }
        public DateTime CreatedAt { get; set; }

        // -------------------------------------------------------
        // Get all fee schedules with Course name joined
        // -------------------------------------------------------
        public static DataTable GetAllFeeSchedules()
        {
            string query = @"SELECT fs.*, c.CourseName 
                             FROM FeeSchedules fs
                             JOIN Courses c ON fs.CourseID = c.CourseID
                             ORDER BY fs.SchoolYear DESC, fs.Semester ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get fee schedules filtered by School Year
        // -------------------------------------------------------
        public static DataTable GetFeeSchedulesByYear(string schoolYear)
        {
            string query = @"SELECT fs.*, c.CourseName 
                             FROM FeeSchedules fs
                             JOIN Courses c ON fs.CourseID = c.CourseID
                             WHERE fs.SchoolYear = @SchoolYear
                             ORDER BY c.CourseName ASC";
            SqlParameter[] param = {
                new SqlParameter("@SchoolYear", schoolYear)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get a single fee schedule by ID
        // -------------------------------------------------------
        public static DataTable GetFeeScheduleByID(int feeScheduleID)
        {
            string query = @"SELECT fs.*, c.CourseName 
                             FROM FeeSchedules fs
                             JOIN Courses c ON fs.CourseID = c.CourseID
                             WHERE fs.FeeScheduleID = @FeeScheduleID";
            SqlParameter[] param = {
                new SqlParameter("@FeeScheduleID", feeScheduleID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get fee schedule by Course, School Year, and Semester
        // Used during assessment to auto-load fees
        // -------------------------------------------------------
        public static DataTable GetFeeScheduleByCourse(int courseID,
                                                        string schoolYear,
                                                        string semester)
        {
            string query = @"SELECT fs.*, c.CourseName 
                             FROM FeeSchedules fs
                             JOIN Courses c ON fs.CourseID = c.CourseID
                             WHERE fs.CourseID = @CourseID
                             AND fs.SchoolYear = @SchoolYear
                             AND fs.Semester = @Semester";
            SqlParameter[] param = {
                new SqlParameter("@CourseID", courseID),
                new SqlParameter("@SchoolYear", schoolYear),
                new SqlParameter("@Semester", semester)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new fee schedule
        // -------------------------------------------------------
        public static int InsertFeeSchedule(FeeScheduleModel f)
        {
            string query = @"INSERT INTO FeeSchedules
                            (CourseID, SchoolYear, Semester, TuitionFeePerUnit,
                             RegistrationFee, MiscellaneousFee, LaboratoryFee)
                             VALUES
                            (@CourseID, @SchoolYear, @Semester, @TuitionFeePerUnit,
                             @RegistrationFee, @MiscellaneousFee, @LaboratoryFee)";

            SqlParameter[] param = {
                new SqlParameter("@CourseID", f.CourseID),
                new SqlParameter("@SchoolYear", f.SchoolYear),
                new SqlParameter("@Semester", f.Semester),
                new SqlParameter("@TuitionFeePerUnit", f.TuitionFeePerUnit),
                new SqlParameter("@RegistrationFee", f.RegistrationFee),
                new SqlParameter("@MiscellaneousFee", f.MiscellaneousFee),
                new SqlParameter("@LaboratoryFee", f.LaboratoryFee)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Update an existing fee schedule
        // -------------------------------------------------------
        public static int UpdateFeeSchedule(FeeScheduleModel f)
        {
            string query = @"UPDATE FeeSchedules SET
                             CourseID = @CourseID,
                             SchoolYear = @SchoolYear,
                             Semester = @Semester,
                             TuitionFeePerUnit = @TuitionFeePerUnit,
                             RegistrationFee = @RegistrationFee,
                             MiscellaneousFee = @MiscellaneousFee,
                             LaboratoryFee = @LaboratoryFee
                             WHERE FeeScheduleID = @FeeScheduleID";

            SqlParameter[] param = {
                new SqlParameter("@FeeScheduleID", f.FeeScheduleID),
                new SqlParameter("@CourseID", f.CourseID),
                new SqlParameter("@SchoolYear", f.SchoolYear),
                new SqlParameter("@Semester", f.Semester),
                new SqlParameter("@TuitionFeePerUnit", f.TuitionFeePerUnit),
                new SqlParameter("@RegistrationFee", f.RegistrationFee),
                new SqlParameter("@MiscellaneousFee", f.MiscellaneousFee),
                new SqlParameter("@LaboratoryFee", f.LaboratoryFee)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Delete a fee schedule by ID
        // -------------------------------------------------------
        public static int DeleteFeeSchedule(int feeScheduleID)
        {
            string query = "DELETE FROM FeeSchedules WHERE FeeScheduleID = @FeeScheduleID";
            SqlParameter[] param = {
                new SqlParameter("@FeeScheduleID", feeScheduleID)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Check if fee schedule already exists for same
        // Course + SchoolYear + Semester (avoid duplicates)
        // -------------------------------------------------------
        public static bool FeeScheduleExists(int courseID, string schoolYear,
                                              string semester, int excludeID = 0)
        {
            string query = @"SELECT COUNT(*) FROM FeeSchedules
                             WHERE CourseID = @CourseID
                             AND SchoolYear = @SchoolYear
                             AND Semester = @Semester
                             AND FeeScheduleID != @ExcludeID";
            SqlParameter[] param = {
                new SqlParameter("@CourseID", courseID),
                new SqlParameter("@SchoolYear", schoolYear),
                new SqlParameter("@Semester", semester),
                new SqlParameter("@ExcludeID", excludeID)
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return count > 0;
        }

        // -------------------------------------------------------
        // Compute total fee based on units enrolled
        // Used during assessment to auto-calculate
        // -------------------------------------------------------
        public static decimal ComputeTotalFee(FeeScheduleModel f, int unitsEnrolled)
        {
            decimal tuition = f.TuitionFeePerUnit * unitsEnrolled;
            decimal total = tuition + f.RegistrationFee
                          + f.MiscellaneousFee + f.LaboratoryFee;
            return total;
        }
    }
}