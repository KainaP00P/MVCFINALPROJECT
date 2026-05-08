using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class DashboardController : Controller
    {
        // -------------------------------------------------------
        // GET: /Dashboard/Index
        // Main dashboard page - shows summary counts and info
        // -------------------------------------------------------
        public ActionResult Index()
        {
            // Redirect to login if not logged in
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            try
            {
                // -------------------------------------------
                // Get total number of enrolled students
                // -------------------------------------------
                object totalStudents = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Students WHERE EnrollmentStatus = 'Enrolled'");
                ViewBag.TotalStudents = Convert.ToInt32(totalStudents);

                // -------------------------------------------
                // Get total number of active courses
                // -------------------------------------------
                object totalCourses = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Courses WHERE IsActive = 1");
                ViewBag.TotalCourses = Convert.ToInt32(totalCourses);

                // -------------------------------------------
                // Get total number of assessments this year
                // -------------------------------------------
                string currentYear = DateTime.Now.Year.ToString();
                object totalAssessments = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM FeeAssessments WHERE SchoolYear LIKE @Year",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@Year", currentYear + "%")
                    });
                ViewBag.TotalAssessments = Convert.ToInt32(totalAssessments);

                // -------------------------------------------
                // Get total collections (sum of all payments)
                // -------------------------------------------
                object totalCollections = DBHelper.ExecuteScalar(
                    "SELECT ISNULL(SUM(AmountPaid), 0) FROM Payments");
                ViewBag.TotalCollections = Convert.ToDecimal(totalCollections);

                // -------------------------------------------
                // Get total number of active scholarships
                // -------------------------------------------
                object totalScholarships = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Scholarships WHERE IsActive = 1");
                ViewBag.TotalScholarships = Convert.ToInt32(totalScholarships);

                // -------------------------------------------
                // Get recent payments (last 5) for the table
                // -------------------------------------------
                DataTable recentPayments = DBHelper.ExecuteQuery(@"
                    SELECT TOP 5
                        p.ORNumber,
                        s.FirstName + ' ' + s.LastName AS StudentName,
                        s.StudentNumber,
                        pt.TermName,
                        p.AmountPaid,
                        p.PaymentDate,
                        p.PaymentMethod
                    FROM Payments p
                    JOIN Students s ON p.StudentID = s.StudentID
                    JOIN PaymentTerms pt ON p.TermID = pt.TermID
                    ORDER BY p.PaymentDate DESC");
                ViewBag.RecentPayments = recentPayments;

                // -------------------------------------------
                // Get recent students (last 5 registered)
                // -------------------------------------------
                DataTable recentStudents = DBHelper.ExecuteQuery(@"
                    SELECT TOP 5
                        s.StudentNumber,
                        s.FirstName + ' ' + s.LastName AS FullName,
                        c.CourseName,
                        s.YearLevel,
                        s.EnrollmentStatus,
                        s.CreatedAt
                    FROM Students s
                    JOIN Courses c ON s.CourseID = c.CourseID
                    ORDER BY s.CreatedAt DESC");
                ViewBag.RecentStudents = recentStudents;

                // -------------------------------------------
                // If logged in as Student, show only their info
                // -------------------------------------------
                if (Session["Role"].ToString() == "Student")
                {
                    int studentID = Convert.ToInt32(Session["StudentID"]);

                    // Get student's own assessment
                    DataTable myAssessment = FeeAssessmentModel
                                            .GetAssessmentsByStudent(studentID);
                    ViewBag.MyAssessment = myAssessment;

                    // Get student's total paid
                    decimal totalPaid = 0;
                    if (myAssessment.Rows.Count > 0)
                    {
                        int assessmentID = Convert.ToInt32(
                                           myAssessment.Rows[0]["AssessmentID"]);
                        totalPaid = PaymentModel.GetTotalPaid(assessmentID);
                    }
                    ViewBag.MyTotalPaid = totalPaid;
                }
            }
            catch (Exception ex)
            {
                // Show error message if something goes wrong
                ViewBag.Error = "Error loading dashboard: " + ex.Message;
            }

            return View();
        }
    }
}