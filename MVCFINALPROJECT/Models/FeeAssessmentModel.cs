using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the FeeAssessments table
    // This is the core of the billing system per student per term
    public class FeeAssessmentModel
    {
        // Properties match the columns in the FeeAssessments table
        public int AssessmentID { get; set; }
        public int StudentID { get; set; }
        public int FeeScheduleID { get; set; }
        public string SchoolYear { get; set; }
        public string Semester { get; set; }
        public int UnitsEnrolled { get; set; }
        public decimal TuitionFee { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal MiscellaneousFee { get; set; }
        public decimal LaboratoryFee { get; set; }
        public decimal TotalAssessment { get; set; }
        public int? ScholarshipID { get; set; }      // nullable, not all students have scholarship
        public int? DiscountID { get; set; }          // nullable, not all students have discount
        public decimal ScholarshipDeduction { get; set; }
        public decimal DiscountDeduction { get; set; }
        public decimal NetAssessment { get; set; }    // final amount to pay
        public int AssessedBy { get; set; }
        public DateTime AssessedAt { get; set; }

        // Extra properties from joined tables (not in FeeAssessments table)
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public string CourseName { get; set; }

        // -------------------------------------------------------
        // Payment term percentages (Prelim/Mid/Semi/Final)
        // -------------------------------------------------------
        public const decimal PrelimPercent = 53m;
        public const decimal MidtermPercent = 64m;
        public const decimal SemiFinalPercent = 75m;
        public const decimal FinalPercent = 100m;

        // -------------------------------------------------------
        // Get all assessments with student and course info joined
        // -------------------------------------------------------
        public static DataTable GetAllAssessments()
        {
            string query = @"SELECT fa.*,
                             s.StudentNumber,
                             s.FirstName + ' ' + s.LastName AS StudentName,
                             c.CourseName
                             FROM FeeAssessments fa
                             JOIN Students s ON fa.StudentID = s.StudentID
                             JOIN Courses c ON s.CourseID = c.CourseID
                             ORDER BY fa.AssessedAt DESC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get a single assessment by AssessmentID
        // -------------------------------------------------------
        public static DataTable GetAssessmentByID(int assessmentID)
        {
            string query = @"SELECT fa.*,
                             s.StudentNumber,
                             s.FirstName + ' ' + s.LastName AS StudentName,
                             c.CourseName
                             FROM FeeAssessments fa
                             JOIN Students s ON fa.StudentID = s.StudentID
                             JOIN Courses c ON s.CourseID = c.CourseID
                             WHERE fa.AssessmentID = @AssessmentID";
            SqlParameter[] param = {
                new SqlParameter("@AssessmentID", assessmentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get all assessments for a specific student
        // -------------------------------------------------------
        public static DataTable GetAssessmentsByStudent(int studentID)
        {
            string query = @"SELECT fa.*,
                             s.StudentNumber,
                             s.FirstName + ' ' + s.LastName AS StudentName,
                             c.CourseName
                             FROM FeeAssessments fa
                             JOIN Students s ON fa.StudentID = s.StudentID
                             JOIN Courses c ON s.CourseID = c.CourseID
                             WHERE fa.StudentID = @StudentID
                             ORDER BY fa.AssessedAt DESC";
            SqlParameter[] param = {
                new SqlParameter("@StudentID", studentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new assessment record
        // -------------------------------------------------------
        public static int InsertAssessment(FeeAssessmentModel f)
        {
            string query = @"INSERT INTO FeeAssessments
                            (StudentID, FeeScheduleID, SchoolYear, Semester,
                             UnitsEnrolled, TuitionFee, RegistrationFee,
                             MiscellaneousFee, LaboratoryFee, TotalAssessment,
                             ScholarshipID, DiscountID, ScholarshipDeduction,
                             DiscountDeduction, NetAssessment, AssessedBy)
                             VALUES
                            (@StudentID, @FeeScheduleID, @SchoolYear, @Semester,
                             @UnitsEnrolled, @TuitionFee, @RegistrationFee,
                             @MiscellaneousFee, @LaboratoryFee, @TotalAssessment,
                             @ScholarshipID, @DiscountID, @ScholarshipDeduction,
                             @DiscountDeduction, @NetAssessment, @AssessedBy);
                             -- Return the new AssessmentID for PaymentTerms creation
                             SELECT SCOPE_IDENTITY();";

            SqlParameter[] param = {
                new SqlParameter("@StudentID", f.StudentID),
                new SqlParameter("@FeeScheduleID", f.FeeScheduleID),
                new SqlParameter("@SchoolYear", f.SchoolYear),
                new SqlParameter("@Semester", f.Semester),
                new SqlParameter("@UnitsEnrolled", f.UnitsEnrolled),
                new SqlParameter("@TuitionFee", f.TuitionFee),
                new SqlParameter("@RegistrationFee", f.RegistrationFee),
                new SqlParameter("@MiscellaneousFee", f.MiscellaneousFee),
                new SqlParameter("@LaboratoryFee", f.LaboratoryFee),
                new SqlParameter("@TotalAssessment", f.TotalAssessment),
                // Use DBNull if no scholarship or discount applied
                new SqlParameter("@ScholarshipID", (object)f.ScholarshipID ?? DBNull.Value),
                new SqlParameter("@DiscountID", (object)f.DiscountID ?? DBNull.Value),
                new SqlParameter("@ScholarshipDeduction", f.ScholarshipDeduction),
                new SqlParameter("@DiscountDeduction", f.DiscountDeduction),
                new SqlParameter("@NetAssessment", f.NetAssessment),
                new SqlParameter("@AssessedBy", f.AssessedBy)
            };
            // Returns the new AssessmentID using SCOPE_IDENTITY
            return Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
        }

        // -------------------------------------------------------
        // Insert payment terms after assessment is created
        // Creates 4 rows: Prelim, Midterm, Semi-Final, Final
        // -------------------------------------------------------
        public static void InsertPaymentTerms(int assessmentID, decimal netAssessment)
        {
            // Each term amount is based on percentage of NetAssessment
            string query = @"INSERT INTO PaymentTerms 
                            (AssessmentID, TermName, PercentDue, AmountDue)
                             VALUES
                            (@AssessmentID, @TermName, @PercentDue, @AmountDue)";

            // Define all 4 terms with their percentages
            var terms = new[]
            {
                new { Name = "Prelim",     Percent = PrelimPercent },
                new { Name = "Midterm",    Percent = MidtermPercent },
                new { Name = "Semi-Final", Percent = SemiFinalPercent },
                new { Name = "Final",      Percent = FinalPercent }
            };

            foreach (var term in terms)
            {
                // Compute amount due for this term
                decimal amountDue = (term.Percent / 100) * netAssessment;

                SqlParameter[] param = {
                    new SqlParameter("@AssessmentID", assessmentID),
                    new SqlParameter("@TermName", term.Name),
                    new SqlParameter("@PercentDue", term.Percent),
                    new SqlParameter("@AmountDue", Math.Round(amountDue, 2))
                };
                DBHelper.ExecuteNonQuery(query, param);
            }
        }

        // -------------------------------------------------------
        // Get payment terms for a specific assessment
        // -------------------------------------------------------
        public static DataTable GetPaymentTerms(int assessmentID)
        {
            string query = @"SELECT * FROM PaymentTerms 
                             WHERE AssessmentID = @AssessmentID
                             ORDER BY PercentDue ASC";
            SqlParameter[] param = {
                new SqlParameter("@AssessmentID", assessmentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Compute all fee breakdown given schedule and units
        // Returns a filled FeeAssessmentModel (not yet saved)
        // -------------------------------------------------------
        public static FeeAssessmentModel ComputeAssessment(
            FeeScheduleModel schedule,
            int unitsEnrolled,
            decimal scholarshipDeduction = 0,
            decimal discountDeduction = 0)
        {
            FeeAssessmentModel result = new FeeAssessmentModel();

            // Compute individual fees
            result.TuitionFee = schedule.TuitionFeePerUnit * unitsEnrolled;
            result.RegistrationFee = schedule.RegistrationFee;
            result.MiscellaneousFee = schedule.MiscellaneousFee;
            result.LaboratoryFee = schedule.LaboratoryFee;

            // Sum all fees
            result.TotalAssessment = result.TuitionFee
                                    + result.RegistrationFee
                                    + result.MiscellaneousFee
                                    + result.LaboratoryFee;

            // Apply deductions
            result.ScholarshipDeduction = scholarshipDeduction;
            result.DiscountDeduction = discountDeduction;

            // Final amount student needs to pay
            result.NetAssessment = result.TotalAssessment
                                 - result.ScholarshipDeduction
                                 - result.DiscountDeduction;

            return result;
        }
    }
}