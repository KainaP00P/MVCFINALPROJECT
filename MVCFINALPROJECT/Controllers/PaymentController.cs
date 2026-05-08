using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class PaymentController : Controller
    {
        // -------------------------------------------------------
        // GET: /Payment/Index
        // Shows list of all payments (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Get all payments with student and term info
                DataTable dt = PaymentModel.GetAllPayments();
                ViewBag.Payments = dt;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading payments: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Payment/Create?assessmentID=5
        // Shows payment form for a specific assessment (Admin only)
        // -------------------------------------------------------
        [HttpGet]
        public ActionResult Create(int assessmentID)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Load assessment details
                DataTable assessmentDT = FeeAssessmentModel
                                         .GetAssessmentByID(assessmentID);

                if (assessmentDT.Rows.Count == 0)
                {
                    TempData["Error"] = "Assessment not found.";
                    return RedirectToAction("Index", "Assessment");
                }

                ViewBag.Assessment = assessmentDT.Rows[0];

                // Load unpaid payment terms for this assessment
                // Only show terms that have not been paid yet
                DataTable termsDT = DBHelper.ExecuteQuery(
                    @"SELECT * FROM PaymentTerms 
                      WHERE AssessmentID = @AssessmentID 
                      AND IsPaid = 0
                      ORDER BY PercentDue ASC",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter(
                            "@AssessmentID", assessmentID)
                    });

                ViewBag.UnpaidTerms = termsDT;

                // Compute total paid and remaining balance
                decimal totalPaid = PaymentModel.GetTotalPaid(assessmentID);
                decimal netAssessment = Convert.ToDecimal(
                                        assessmentDT.Rows[0]["NetAssessment"]);

                ViewBag.TotalPaid = totalPaid;
                ViewBag.RemainingBalance = netAssessment - totalPaid;

                // Auto-generate OR number for this payment
                ViewBag.ORNumber = PaymentModel.GenerateORNumber();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading payment form: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // POST: /Payment/Create
        // Processes a new payment record (Admin only)
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                int assessmentID = Convert.ToInt32(form["AssessmentID"]);
                int termID = Convert.ToInt32(form["TermID"]);
                string orNumber = form["ORNumber"].Trim();

                // Check if OR number already exists
                if (PaymentModel.ORNumberExists(orNumber))
                {
                    TempData["Error"] = "OR Number already exists. " +
                                        "Please use a different OR number.";
                    return RedirectToAction("Create",
                                            new { assessmentID = assessmentID });
                }

                // Get the selected term to validate amount
                DataTable termDT = DBHelper.ExecuteQuery(
                    "SELECT * FROM PaymentTerms WHERE TermID = @TermID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@TermID", termID)
                    });

                if (termDT.Rows.Count == 0)
                {
                    TempData["Error"] = "Payment term not found.";
                    return RedirectToAction("Create",
                                            new { assessmentID = assessmentID });
                }

                decimal amountDue = Convert.ToDecimal(termDT.Rows[0]["AmountDue"]);
                decimal amountPaid = Convert.ToDecimal(form["AmountPaid"]);

                // Validate amount paid is not less than amount due
                if (amountPaid < amountDue)
                {
                    TempData["Error"] = $"Amount paid must be at least " +
                                        $"₱{amountDue:N2} for this term.";
                    return RedirectToAction("Create",
                                            new { assessmentID = assessmentID });
                }

                // Get studentID from assessment
                DataTable assessmentDT = FeeAssessmentModel
                                         .GetAssessmentByID(assessmentID);
                int studentID = Convert.ToInt32(assessmentDT.Rows[0]["StudentID"]);

                // Build payment model from form data
                PaymentModel payment = new PaymentModel
                {
                    StudentID = studentID,
                    AssessmentID = assessmentID,
                    TermID = termID,
                    AmountPaid = amountPaid,
                    PaymentDate = Convert.ToDateTime(form["PaymentDate"]),
                    PaymentMethod = form["PaymentMethod"],
                    ORNumber = orNumber,
                    Remarks = form["Remarks"],
                    // Admin who received the payment
                    ReceivedBy = Convert.ToInt32(Session["UserID"])
                };

                // Save the payment record
                PaymentModel.InsertPayment(payment);

                // Mark the selected term as paid
                PaymentModel.MarkTermAsPaid(termID);

                TempData["Success"] = "Payment recorded successfully!";

                // Redirect back to assessment details
                return RedirectToAction("Details", "Assessment",
                                         new { id = assessmentID });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error recording payment: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // -------------------------------------------------------
        // GET: /Payment/Details/5
        // Shows details of a single payment (Admin only)
        // -------------------------------------------------------
        public ActionResult Details(int id)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Load payment with joined student and term info
                DataTable dt = PaymentModel.GetPaymentByID(id);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Payment not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.Payment = dt.Rows[0];
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading payment: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Payment/GetTermAmount?termID=5
        // AJAX endpoint - returns term amount due as JSON
        // Called when a term is selected in the payment form
        // -------------------------------------------------------
        public JsonResult GetTermAmount(int termID)
        {
            try
            {
                DataTable dt = DBHelper.ExecuteQuery(
                    "SELECT * FROM PaymentTerms WHERE TermID = @TermID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@TermID", termID)
                    });

                if (dt.Rows.Count == 0)
                    return Json(new { success = false },
                                JsonRequestBehavior.AllowGet);

                // Return term details for auto-filling amount field
                return Json(new
                {
                    success = true,
                    termName = dt.Rows[0]["TermName"].ToString(),
                    amountDue = dt.Rows[0]["AmountDue"].ToString()
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