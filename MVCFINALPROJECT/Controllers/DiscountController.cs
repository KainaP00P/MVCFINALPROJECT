using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class DiscountController : Controller
    {
        // -------------------------------------------------------
        // GET: /Discount/Index
        // Shows list of all discounts (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Get all discounts including inactive
                DataTable dt = DiscountModel.GetAllDiscounts();
                ViewBag.Discounts = dt;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading discounts: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Discount/Create
        // Shows form to add a new discount (Admin only)
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
        // POST: /Discount/Create
        // Processes new discount creation (Admin only)
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
                string discountName = form["DiscountName"].Trim();

                // Check if discount name already exists
                if (DiscountModel.DiscountNameExists(discountName))
                {
                    ViewBag.Error = "Discount name already exists.";
                    return View();
                }

                // Parse amount and percent safely
                // Either one can be zero depending on discount type
                decimal discountAmount = 0;
                decimal discountPercent = 0;

                if (!string.IsNullOrEmpty(form["DiscountAmount"]))
                    discountAmount = Convert.ToDecimal(form["DiscountAmount"]);

                if (!string.IsNullOrEmpty(form["DiscountPercent"]))
                    discountPercent = Convert.ToDecimal(form["DiscountPercent"]);

                // Validate that at least one value is provided
                if (discountAmount == 0 && discountPercent == 0)
                {
                    ViewBag.Error = "Please enter either a discount " +
                                    "amount or a discount percent.";
                    return View();
                }

                // Build new discount model from form data
                DiscountModel discount = new DiscountModel
                {
                    DiscountName = discountName,
                    Description = form["Description"].Trim(),
                    DiscountAmount = discountAmount,
                    DiscountPercent = discountPercent,
                    IsActive = form["IsActive"] == "true"
                };

                DiscountModel.InsertDiscount(discount);

                TempData["Success"] = "Discount added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding discount: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // GET: /Discount/Edit/5
        // Shows form to edit a discount (Admin only)
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
                // Load discount data by ID
                DataTable dt = DiscountModel.GetDiscountByID(id);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Discount not found.";
                    return RedirectToAction("Index");
                }

                // Pass discount data to view
                ViewBag.Discount = dt.Rows[0];
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading discount: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // POST: /Discount/Edit/5
        // Processes discount update (Admin only)
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
                string discountName = form["DiscountName"].Trim();

                // Check if name exists but exclude current record
                if (DiscountModel.DiscountNameExists(discountName, id))
                {
                    ViewBag.Error = "Discount name already exists.";
                    ViewBag.Discount = DiscountModel.GetDiscountByID(id).Rows[0];
                    return View();
                }

                // Parse amount and percent safely
                decimal discountAmount = 0;
                decimal discountPercent = 0;

                if (!string.IsNullOrEmpty(form["DiscountAmount"]))
                    discountAmount = Convert.ToDecimal(form["DiscountAmount"]);

                if (!string.IsNullOrEmpty(form["DiscountPercent"]))
                    discountPercent = Convert.ToDecimal(form["DiscountPercent"]);

                // Validate at least one value
                if (discountAmount == 0 && discountPercent == 0)
                {
                    ViewBag.Error = "Please enter either a discount " +
                                    "amount or a discount percent.";
                    ViewBag.Discount = DiscountModel.GetDiscountByID(id).Rows[0];
                    return View();
                }

                // Build updated discount model
                DiscountModel discount = new DiscountModel
                {
                    DiscountID = id,
                    DiscountName = discountName,
                    Description = form["Description"].Trim(),
                    DiscountAmount = discountAmount,
                    DiscountPercent = discountPercent,
                    IsActive = form["IsActive"] == "true"
                };

                DiscountModel.UpdateDiscount(discount);

                TempData["Success"] = "Discount updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating discount: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // POST: /Discount/Delete/5
        // Deletes a discount (Admin only)
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
                // Check if discount is used in any assessment
                object count = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM FeeAssessments WHERE DiscountID = @ID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@ID", id)
                    });

                if (Convert.ToInt32(count) > 0)
                {
                    TempData["Error"] = "Cannot delete. Discount is " +
                                        "already used in an assessment.";
                    return RedirectToAction("Index");
                }

                DiscountModel.DeleteDiscount(id);
                TempData["Success"] = "Discount deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting discount: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        // POST: /Discount/ToggleStatus/5
        // Activates or deactivates a discount
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
                    @"UPDATE Discounts
                      SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                      WHERE DiscountID = @ID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@ID", id)
                    });

                TempData["Success"] = "Discount status updated!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating status: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}