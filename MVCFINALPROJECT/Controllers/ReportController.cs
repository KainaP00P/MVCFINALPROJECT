using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class ReportController : Controller
    {
        // -------------------------------------------------------
        // GET: /Report/Index
        // Shows report menu page (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // -------------------------------------------------------
        // GET: /Report/StudentLedger
        // Shows student ledger report with optional filters
        // Displays charges, payments, discounts, and balance
        // -------------------------------------------------------
        public ActionResult StudentLedger(string studentNumber = "",
                                          string schoolYear = "")
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                DataTable dt;

                // If filters are provided, apply them
                if (!string.IsNullOrEmpty(studentNumber) ||
                    !string.IsNullOrEmpty(schoolYear))
                {
                    string query = @"SELECT * FROM vw_StudentLedger WHERE 1=1";

                    // Dynamically build filter conditions
                    var parameters = new System.Collections.Generic
                                         .List<System.Data.SqlClient.SqlParameter>();

                    if (!string.IsNullOrEmpty(studentNumber))
                    {
                        query += " AND StudentNumber LIKE @StudentNumber";
                        parameters.Add(new System.Data.SqlClient.SqlParameter(
                            "@StudentNumber", "%" + studentNumber + "%"));
                    }

                    if (!string.IsNullOrEmpty(schoolYear))
                    {
                        query += " AND SchoolYear = @SchoolYear";
                        parameters.Add(new System.Data.SqlClient.SqlParameter(
                            "@SchoolYear", schoolYear));
                    }

                    query += " ORDER BY FullName ASC";
                    dt = DBHelper.ExecuteQuery(query, parameters.ToArray());
                }
                else
                {
                    // No filters - get all ledger records
                    dt = PaymentModel.GetAllStudentLedgers();
                }

                ViewBag.LedgerData = dt;
                ViewBag.StudentNumber = studentNumber;
                ViewBag.SchoolYear = schoolYear;

                // Compute grand totals for the report footer
                decimal grandTotalAssessment = 0;
                decimal grandTotalPaid = 0;
                decimal grandTotalBalance = 0;

                foreach (DataRow row in dt.Rows)
                {
                    grandTotalAssessment += Convert.ToDecimal(row["NetAssessment"]);
                    grandTotalPaid += Convert.ToDecimal(row["TotalPaid"]);
                    grandTotalBalance += Convert.ToDecimal(row["RemainingBalance"]);
                }

                ViewBag.GrandTotalAssessment = grandTotalAssessment;
                ViewBag.GrandTotalPaid = grandTotalPaid;
                ViewBag.GrandTotalBalance = grandTotalBalance;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading ledger report: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Report/AssessmentReport
        // Shows assessment report per student
        // Displays assessed tuition and other fees per student
        // -------------------------------------------------------
        public ActionResult AssessmentReport(string schoolYear = "",
                                              string semester = "",
                                              int courseID = 0)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Build dynamic query based on filters
                string query = @"SELECT 
                                fa.AssessmentID,
                                s.StudentNumber,
                                s.FirstName + ' ' + s.LastName AS StudentName,
                                c.CourseName,
                                fa.SchoolYear,
                                fa.Semester,
                                fa.UnitsEnrolled,
                                fa.TuitionFee,
                                fa.RegistrationFee,
                                fa.MiscellaneousFee,
                                fa.LaboratoryFee,
                                fa.TotalAssessment,
                                fa.ScholarshipDeduction,
                                fa.DiscountDeduction,
                                fa.NetAssessment,
                                fa.AssessedAt
                                FROM FeeAssessments fa
                                JOIN Students s ON fa.StudentID = s.StudentID
                                JOIN Courses c ON s.CourseID = c.CourseID
                                WHERE 1=1";

                var parameters = new System.Collections.Generic
                                     .List<System.Data.SqlClient.SqlParameter>();

                // Apply school year filter if provided
                if (!string.IsNullOrEmpty(schoolYear))
                {
                    query += " AND fa.SchoolYear = @SchoolYear";
                    parameters.Add(new System.Data.SqlClient.SqlParameter(
                        "@SchoolYear", schoolYear));
                }

                // Apply semester filter if provided
                if (!string.IsNullOrEmpty(semester))
                {
                    query += " AND fa.Semester = @Semester";
                    parameters.Add(new System.Data.SqlClient.SqlParameter(
                        "@Semester", semester));
                }

                // Apply course filter if provided
                if (courseID > 0)
                {
                    query += " AND s.CourseID = @CourseID";
                    parameters.Add(new System.Data.SqlClient.SqlParameter(
                        "@CourseID", courseID));
                }

                query += " ORDER BY s.LastName ASC, fa.AssessedAt DESC";

                DataTable dt = DBHelper.ExecuteQuery(query, parameters.ToArray());
                ViewBag.AssessmentData = dt;

                // Pass filter values back to view for form persistence
                ViewBag.SchoolYear = schoolYear;
                ViewBag.Semester = semester;
                ViewBag.CourseID = courseID;

                // Load courses for filter dropdown
                ViewBag.Courses = CourseModel.GetAllCourses();

                // Compute grand totals for report footer
                decimal grandTotalAssessment = 0;
                decimal grandNetAssessment = 0;
                decimal grandScholarship = 0;
                decimal grandDiscount = 0;

                foreach (DataRow row in dt.Rows)
                {
                    grandTotalAssessment += Convert.ToDecimal(row["TotalAssessment"]);
                    grandNetAssessment += Convert.ToDecimal(row["NetAssessment"]);
                    grandScholarship += Convert.ToDecimal(row["ScholarshipDeduction"]);
                    grandDiscount += Convert.ToDecimal(row["DiscountDeduction"]);
                }

                ViewBag.GrandTotalAssessment = grandTotalAssessment;
                ViewBag.GrandNetAssessment = grandNetAssessment;
                ViewBag.GrandScholarship = grandScholarship;
                ViewBag.GrandDiscount = grandDiscount;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading assessment report: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Report/StudentLedgerDetail/5
        // Shows detailed ledger for a single student
        // -------------------------------------------------------
        public ActionResult StudentLedgerDetail(int studentID)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            try
            {
                // Load student info
                DataTable studentDT = StudentModel.GetStudentByID(studentID);

                if (studentDT.Rows.Count == 0)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("StudentLedger");
                }

                ViewBag.Student = studentDT.Rows[0];

                // Load all assessments for this student
                DataTable assessmentDT = FeeAssessmentModel
                                         .GetAssessmentsByStudent(studentID);
                ViewBag.Assessments = assessmentDT;

                // Load all payments for this student
                DataTable paymentDT = PaymentModel
                                      .GetPaymentsByStudent(studentID);
                ViewBag.Payments = paymentDT;

                // Compute overall totals
                decimal totalAssessed = 0;
                decimal totalPaid = 0;

                foreach (DataRow row in assessmentDT.Rows)
                    totalAssessed += Convert.ToDecimal(row["NetAssessment"]);

                foreach (DataRow row in paymentDT.Rows)
                    totalPaid += Convert.ToDecimal(row["AmountPaid"]);

                ViewBag.TotalAssessed = totalAssessed;
                ViewBag.TotalPaid = totalPaid;
                ViewBag.OverallBalance = totalAssessed - totalPaid;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading student ledger: " + ex.Message;
            }

            return View();
        }
    }
}