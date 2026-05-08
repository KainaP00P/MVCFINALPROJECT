using System;
using System.Data;
using System.Data.SqlClient;

namespace MVCFINALPROJECT.Models
{
    // Represents one row in the Payments table
    // Records every actual payment made by a student
    public class PaymentModel
    {
        // Properties match the columns in the Payments table
        public int PaymentID { get; set; }
        public int StudentID { get; set; }
        public int AssessmentID { get; set; }
        public int TermID { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }  // Cash, Online, etc.
        public string ORNumber { get; set; }        // Official Receipt Number
        public string Remarks { get; set; }
        public int ReceivedBy { get; set; }         // UserID of admin who received

        // Extra properties from joined tables (not in Payments table)
        public string StudentName { get; set; }
        public string StudentNumber { get; set; }
        public string TermName { get; set; }        // Prelim, Midterm, etc.
        public string ReceivedByName { get; set; }  // Username of admin

        // -------------------------------------------------------
        // Get all payments with student and term info joined
        // -------------------------------------------------------
        public static DataTable GetAllPayments()
        {
            string query = @"SELECT p.*,
                             s.StudentNumber,
                             s.FirstName + ' ' + s.LastName AS StudentName,
                             pt.TermName,
                             u.Username AS ReceivedByName
                             FROM Payments p
                             JOIN Students s ON p.StudentID = s.StudentID
                             JOIN PaymentTerms pt ON p.TermID = pt.TermID
                             JOIN Users u ON p.ReceivedBy = u.UserID
                             ORDER BY p.PaymentDate DESC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Get all payments for a specific student
        // -------------------------------------------------------
        public static DataTable GetPaymentsByStudent(int studentID)
        {
            string query = @"SELECT p.*,
                             s.StudentNumber,
                             s.FirstName + ' ' + s.LastName AS StudentName,
                             pt.TermName,
                             pt.AmountDue,
                             u.Username AS ReceivedByName
                             FROM Payments p
                             JOIN Students s ON p.StudentID = s.StudentID
                             JOIN PaymentTerms pt ON p.TermID = pt.TermID
                             JOIN Users u ON p.ReceivedBy = u.UserID
                             WHERE p.StudentID = @StudentID
                             ORDER BY p.PaymentDate DESC";
            SqlParameter[] param = {
                new SqlParameter("@StudentID", studentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get all payments for a specific assessment
        // -------------------------------------------------------
        public static DataTable GetPaymentsByAssessment(int assessmentID)
        {
            string query = @"SELECT p.*,
                             pt.TermName,
                             pt.AmountDue,
                             u.Username AS ReceivedByName
                             FROM Payments p
                             JOIN PaymentTerms pt ON p.TermID = pt.TermID
                             JOIN Users u ON p.ReceivedBy = u.UserID
                             WHERE p.AssessmentID = @AssessmentID
                             ORDER BY p.PaymentDate ASC";
            SqlParameter[] param = {
                new SqlParameter("@AssessmentID", assessmentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get a single payment by PaymentID
        // -------------------------------------------------------
        public static DataTable GetPaymentByID(int paymentID)
        {
            string query = @"SELECT p.*,
                             s.StudentNumber,
                             s.FirstName + ' ' + s.LastName AS StudentName,
                             pt.TermName,
                             pt.AmountDue,
                             u.Username AS ReceivedByName
                             FROM Payments p
                             JOIN Students s ON p.StudentID = s.StudentID
                             JOIN PaymentTerms pt ON p.TermID = pt.TermID
                             JOIN Users u ON p.ReceivedBy = u.UserID
                             WHERE p.PaymentID = @PaymentID";
            SqlParameter[] param = {
                new SqlParameter("@PaymentID", paymentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Insert a new payment record
        // -------------------------------------------------------
        public static int InsertPayment(PaymentModel p)
        {
            string query = @"INSERT INTO Payments
                            (StudentID, AssessmentID, TermID, AmountPaid,
                             PaymentDate, PaymentMethod, ORNumber, 
                             Remarks, ReceivedBy)
                             VALUES
                            (@StudentID, @AssessmentID, @TermID, @AmountPaid,
                             @PaymentDate, @PaymentMethod, @ORNumber,
                             @Remarks, @ReceivedBy)";

            SqlParameter[] param = {
                new SqlParameter("@StudentID", p.StudentID),
                new SqlParameter("@AssessmentID", p.AssessmentID),
                new SqlParameter("@TermID", p.TermID),
                new SqlParameter("@AmountPaid", p.AmountPaid),
                new SqlParameter("@PaymentDate", p.PaymentDate),
                new SqlParameter("@PaymentMethod", p.PaymentMethod),
                new SqlParameter("@ORNumber", p.ORNumber),
                new SqlParameter("@Remarks", (object)p.Remarks ?? DBNull.Value),
                new SqlParameter("@ReceivedBy", p.ReceivedBy)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Mark a payment term as paid after payment is recorded
        // -------------------------------------------------------
        public static int MarkTermAsPaid(int termID)
        {
            string query = @"UPDATE PaymentTerms 
                             SET IsPaid = 1 
                             WHERE TermID = @TermID";
            SqlParameter[] param = {
                new SqlParameter("@TermID", termID)
            };
            return DBHelper.ExecuteNonQuery(query, param);
        }

        // -------------------------------------------------------
        // Get total amount paid for a specific assessment
        // Used to compute remaining balance
        // -------------------------------------------------------
        public static decimal GetTotalPaid(int assessmentID)
        {
            string query = @"SELECT ISNULL(SUM(AmountPaid), 0) 
                             FROM Payments 
                             WHERE AssessmentID = @AssessmentID";
            SqlParameter[] param = {
                new SqlParameter("@AssessmentID", assessmentID)
            };
            return Convert.ToDecimal(DBHelper.ExecuteScalar(query, param));
        }

        // -------------------------------------------------------
        // Get student ledger data (used for Ledger Report)
        // Pulls from the vw_StudentLedger view
        // -------------------------------------------------------
        public static DataTable GetStudentLedger(int studentID)
        {
            string query = @"SELECT * FROM vw_StudentLedger 
                             WHERE StudentNumber = (
                                SELECT StudentNumber 
                                FROM Students 
                                WHERE StudentID = @StudentID)";
            SqlParameter[] param = {
                new SqlParameter("@StudentID", studentID)
            };
            return DBHelper.ExecuteQuery(query, param);
        }

        // -------------------------------------------------------
        // Get full ledger for all students (admin report)
        // -------------------------------------------------------
        public static DataTable GetAllStudentLedgers()
        {
            string query = "SELECT * FROM vw_StudentLedger ORDER BY FullName ASC";
            return DBHelper.ExecuteQuery(query);
        }

        // -------------------------------------------------------
        // Check if OR Number already exists (avoid duplicates)
        // -------------------------------------------------------
        public static bool ORNumberExists(string orNumber)
        {
            string query = @"SELECT COUNT(*) FROM Payments 
                             WHERE ORNumber = @ORNumber";
            SqlParameter[] param = {
                new SqlParameter("@ORNumber", orNumber)
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return count > 0;
        }

        // -------------------------------------------------------
        // Auto-generate OR Number
        // Format: OR-YYYYMMDD-XXXXX (e.g. OR-20240101-00001)
        // -------------------------------------------------------
        public static string GenerateORNumber()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            string query = @"SELECT COUNT(*) FROM Payments 
                             WHERE ORNumber LIKE @Date";
            SqlParameter[] param = {
                new SqlParameter("@Date", "OR-" + date + "%")
            };
            int count = Convert.ToInt32(DBHelper.ExecuteScalar(query, param));
            return "OR-" + date + "-" + (count + 1).ToString("D5");
        }
    }
}