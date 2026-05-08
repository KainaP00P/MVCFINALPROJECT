using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class AssessmentController : Controller
    {
        // -------------------------------------------------------
        // GET: /Assessment/Index
        // Shows list of all assessments (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Get all assessments with student and course info
                DataTable dt = FeeAssessmentModel.GetAllAssessments();
                ViewBag.Assessments = dt;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading assessments: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Assessment/Create
        // Shows form to create a new assessment (Admin only)
        // -------------------------------------------------------
        [HttpGet]
        public ActionResult Create()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            // Load dropdowns needed by the assessment form
            ViewBag.Students = StudentModel.GetAllStudents();
            ViewBag.Scholarships = ScholarshipModel.GetActiveScholarships();
            ViewBag.Discounts = DiscountModel.GetActiveDiscounts();

            // Default school year
            ViewBag.DefaultSchoolYear = DateTime.Now.Year.ToString()
                                      + "-"
                                      + (DateTime.Now.Year + 1).ToString();
            return View();
        }

        // -------------------------------------------------------
        // POST: /Assessment/Create
        // Processes new assessment creation (Admin only)
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            // Reload dropdowns in case of error
            ViewBag.Students = StudentModel.GetAllStudents();
            ViewBag.Scholarships = ScholarshipModel.GetActiveScholarships();
            ViewBag.Discounts = DiscountModel.GetActiveDiscounts();

            try
            {
                int studentID = Convert.ToInt32(form["StudentID"]);
                int feeScheduleID = Convert.ToInt32(form["FeeScheduleID"]);
                int unitsEnrolled = Convert.ToInt32(form["UnitsEnrolled"]);
                string schoolYear = form["SchoolYear"].Trim();
                string semester = form["Semester"];

                // Check if student already has assessment
                // for the same school year and semester
                object existingCount = DBHelper.ExecuteScalar(
                    @"SELECT COUNT(*) FROM FeeAssessments 
                      WHERE StudentID = @StudentID 
                      AND SchoolYear = @SchoolYear 
                      AND Semester = @Semester",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@StudentID", studentID),
                        new System.Data.SqlClient.SqlParameter("@SchoolYear", schoolYear),
                        new System.Data.SqlClient.SqlParameter("@Semester", semester)
                    });

                if (Convert.ToInt32(existingCount) > 0)
                {
                    ViewBag.Error = "Student already has an assessment " +
                                    "for this school year and semester.";
                    return View();
                }

                // Get fee schedule to compute fees
                DataTable scheduleDT = FeeScheduleModel
                                       .GetFeeScheduleByID(feeScheduleID);

                if (scheduleDT.Rows.Count == 0)
                {
                    ViewBag.Error = "Fee schedule not found.";
                    return View();
                }

                // Build fee schedule model for computation
                FeeScheduleModel schedule = new FeeScheduleModel
                {
                    FeeScheduleID = feeScheduleID,
                    TuitionFeePerUnit = Convert.ToDecimal(
                                        scheduleDT.Rows[0]["TuitionFeePerUnit"]),
                    RegistrationFee = Convert.ToDecimal(
                                        scheduleDT.Rows[0]["RegistrationFee"]),
                    MiscellaneousFee = Convert.ToDecimal(
                                        scheduleDT.Rows[0]["MiscellaneousFee"]),
                    LaboratoryFee = Convert.ToDecimal(
                                        scheduleDT.Rows[0]["LaboratoryFee"])
                };

                // Handle optional scholarship deduction
                int? scholarshipID = null;
                decimal scholarshipDeduction = 0;

                if (!string.IsNullOrEmpty(form["ScholarshipID"])
                    && form["ScholarshipID"] != "0")
                {
                    scholarshipID = Convert.ToInt32(form["ScholarshipID"]);
                    DataTable scholarshipDT = ScholarshipModel
                                             .GetScholarshipByID(scholarshipID.Value);

                    if (scholarshipDT.Rows.Count > 0)
                    {
                        decimal percent = Convert.ToDecimal(
                                          scholarshipDT.Rows[0]["DiscountPercent"]);

                        // Compute scholarship deduction from total fee
                        decimal totalFee = FeeScheduleModel
                                           .ComputeTotalFee(schedule, unitsEnrolled);
                        scholarshipDeduction = (percent / 100) * totalFee;
                    }
                }

                // Handle optional discount deduction
                int? discountID = null;
                decimal discountDeduction = 0;

                if (!string.IsNullOrEmpty(form["DiscountID"])
                    && form["DiscountID"] != "0")
                {
                    discountID = Convert.ToInt32(form["DiscountID"]);
                    DataTable discountDT = DiscountModel
                                          .GetDiscountByID(discountID.Value);

                    if (discountDT.Rows.Count > 0)
                    {
                        DiscountModel discount = new DiscountModel
                        {
                            DiscountAmount = Convert.ToDecimal(
                                             discountDT.Rows[0]["DiscountAmount"]),
                            DiscountPercent = Convert.ToDecimal(
                                             discountDT.Rows[0]["DiscountPercent"])
                        };

                        decimal totalFee = FeeScheduleModel
                                           .ComputeTotalFee(schedule, unitsEnrolled);
                        discountDeduction = DiscountModel
                                            .ComputeDeduction(discount, totalFee);
                    }
                }

                // Compute full assessment using the model method
                FeeAssessmentModel assessment = FeeAssessmentModel.ComputeAssessment(
                    schedule, unitsEnrolled, scholarshipDeduction, discountDeduction);

                // Fill in remaining fields
                assessment.StudentID = studentID;
                assessment.FeeScheduleID = feeScheduleID;
                assessment.SchoolYear = schoolYear;
                assessment.Semester = semester;
                assessment.UnitsEnrolled = unitsEnrolled;
                assessment.ScholarshipID = scholarshipID;
                assessment.DiscountID = discountID;
                assessment.AssessedBy = Convert.ToInt32(Session["UserID"]);

                // Save assessment and get the new AssessmentID
                int newAssessmentID = FeeAssessmentModel.InsertAssessment(assessment);

                // Auto-create the 4 payment terms (Prelim/Mid/Semi/Final)
                FeeAssessmentModel.InsertPaymentTerms(
                    newAssessmentID, assessment.NetAssessment);

                TempData["Success"] = "Assessment created successfully!";
                return RedirectToAction("Details", new { id = newAssessmentID });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error creating assessment: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // GET: /Assessment/Details/5
        // Shows full assessment with payment terms breakdown
        // -------------------------------------------------------
        public ActionResult Details(int id)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            try
            {
                // Load assessment data
                DataTable assessmentDT = FeeAssessmentModel.GetAssessmentByID(id);

                if (assessmentDT.Rows.Count == 0)
                {
                    TempData["Error"] = "Assessment not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.Assessment = assessmentDT.Rows[0];

                // Load the 4 payment terms for this assessment
                DataTable termsDT = FeeAssessmentModel.GetPaymentTerms(id);
                ViewBag.PaymentTerms = termsDT;

                // Load payment history for this assessment
                DataTable paymentsDT = PaymentModel.GetPaymentsByAssessment(id);
                ViewBag.Payments = paymentsDT;

                // Compute total paid and remaining balance
                decimal totalPaid = PaymentModel.GetTotalPaid(id);
                ViewBag.TotalPaid = totalPaid;

                decimal netAssessment = Convert.ToDecimal(
                                        assessmentDT.Rows[0]["NetAssessment"]);
                ViewBag.RemainingBalance = netAssessment - totalPaid;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading assessment: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Assessment/GetStudentInfo
        // AJAX endpoint - returns student course info as JSON
        // Called when student is selected in assessment form
        // -------------------------------------------------------
        public JsonResult GetStudentInfo(int studentID)
        {
            try
            {
                DataTable dt = StudentModel.GetStudentByID(studentID);

                if (dt.Rows.Count == 0)
                    return Json(new { success = false },
                                JsonRequestBehavior.AllowGet);

                DataRow row = dt.Rows[0];

                return Json(new
                {
                    success = true,
                    courseID = row["CourseID"].ToString(),
                    courseName = row["CourseName"].ToString(),
                    yearLevel = row["YearLevel"].ToString(),
                    semester = row["Semester"].ToString(),
                    schoolYear = row["SchoolYear"].ToString()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message },
                            JsonRequestBehavior.AllowGet);
            }
        }
    }
}