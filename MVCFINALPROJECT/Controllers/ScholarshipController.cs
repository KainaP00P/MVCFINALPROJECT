using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class ScholarshipController : Controller
    {
        // -------------------------------------------------------
        // GET: /Scholarship/Index
        // Shows list of all scholarships (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Get all scholarships including inactive
                DataTable dt = ScholarshipModel.GetAllScholarships();
                ViewBag.Scholarships = dt;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading scholarships: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Scholarship/Create
        // Shows form to add a new scholarship (Admin only)
        // -------------------------------------------------------
        [HttpGet]
        public ActionResult Create()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // -------------------------------------------------------
        // POST: /Scholarship/Create
        // Processes new scholarship creation (Admin only)
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
                string scholarshipName = form["ScholarshipName"].Trim();

                // Check if scholarship name already exists
                if (ScholarshipModel.ScholarshipNameExists(scholarshipName))
                {
                    ViewBag.Error = "Scholarship name already exists.";
                    return View();
                }

                // Build new scholarship model from form data
                ScholarshipModel scholarship = new ScholarshipModel
                {
                    ScholarshipName = scholarshipName,
                    Description = form["Description"].Trim(),
                    // Discount percent e.g. 100 = full, 50 = half
                    DiscountPercent = Convert.ToDecimal(form["DiscountPercent"]),
                    IsActive = form["IsActive"] == "true"
                };

                ScholarshipModel.InsertScholarship(scholarship);

                TempData["Success"] = "Scholarship added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding scholarship: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // GET: /Scholarship/Edit/5
        // Shows form to edit a scholarship (Admin only)
        // -------------------------------------------------------
        [HttpGet]
        public ActionResult Edit(int id)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Load scholarship data by ID
                DataTable dt = ScholarshipModel.GetScholarshipByID(id);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Scholarship not found.";
                    return RedirectToAction("Index");
                }

                // Pass scholarship data to view
                ViewBag.Scholarship = dt.Rows[0];
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading scholarship: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // POST: /Scholarship/Edit/5
        // Processes scholarship update (Admin only)
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Edit(int id, FormCollection form)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                string scholarshipName = form["ScholarshipName"].Trim();

                // Check if name exists but exclude current record
                if (ScholarshipModel.ScholarshipNameExists(scholarshipName, id))
                {
                    ViewBag.Error = "Scholarship name already exists.";
                    ViewBag.Scholarship = ScholarshipModel
                                         .GetScholarshipByID(id).Rows[0];
                    return View();
                }

                // Build updated scholarship model
                ScholarshipModel scholarship = new ScholarshipModel
                {
                    ScholarshipID = id,
                    ScholarshipName = scholarshipName,
                    Description = form["Description"].Trim(),
                    DiscountPercent = Convert.ToDecimal(form["DiscountPercent"]),
                    IsActive = form["IsActive"] == "true"
                };

                ScholarshipModel.UpdateScholarship(scholarship);

                TempData["Success"] = "Scholarship updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating scholarship: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // POST: /Scholarship/Delete/5
        // Deletes a scholarship (Admin only)
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Check if scholarship is used in any assessment
                object count = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM FeeAssessments WHERE ScholarshipID = @ID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@ID", id)
                    });

                if (Convert.ToInt32(count) > 0)
                {
                    TempData["Error"] = "Cannot delete. Scholarship is " +
                                        "already used in an assessment.";
                    return RedirectToAction("Index");
                }

                ScholarshipModel.DeleteScholarship(id);
                TempData["Success"] = "Scholarship deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting scholarship: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        // POST: /Scholarship/ToggleStatus/5
        // Activates or deactivates a scholarship
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult ToggleStatus(int id)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Flip the IsActive status
                DBHelper.ExecuteNonQuery(
                    @"UPDATE Scholarships
                      SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                      WHERE ScholarshipID = @ID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@ID", id)
                    });

                TempData["Success"] = "Scholarship status updated!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating status: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}