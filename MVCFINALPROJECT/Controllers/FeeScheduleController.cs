using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class FeeScheduleController : Controller
    {
        // -------------------------------------------------------
        // GET: /FeeSchedule/Index
        // Shows list of all fee schedules (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Get all fee schedules with course name joined
                DataTable dt = FeeScheduleModel.GetAllFeeSchedules();
                ViewBag.FeeSchedules = dt;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading fee schedules: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /FeeSchedule/Create
        // Shows form to add a new fee schedule (Admin only)
        // -------------------------------------------------------
        [HttpGet]
        public ActionResult Create()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            // Load active courses for dropdown
            ViewBag.Courses = CourseModel.GetActiveCourses();

            // Set default school year to current year
            ViewBag.DefaultSchoolYear = DateTime.Now.Year.ToString()
                                      + "-"
                                      + (DateTime.Now.Year + 1).ToString();
            return View();
        }

        // -------------------------------------------------------
        // POST: /FeeSchedule/Create
        // Processes new fee schedule creation (Admin only)
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Create(FormCollection form)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            // Reload courses dropdown in case of error
            ViewBag.Courses = CourseModel.GetActiveCourses();

            try
            {
                int courseID = Convert.ToInt32(form["CourseID"]);
                string schoolYear = form["SchoolYear"].Trim();
                string semester = form["Semester"];

                // Check if fee schedule already exists for same
                // Course + SchoolYear + Semester combination
                if (FeeScheduleModel.FeeScheduleExists(courseID, schoolYear, semester))
                {
                    ViewBag.Error = "A fee schedule for this course, " +
                                    "school year, and semester already exists.";
                    return View();
                }

                // Build new fee schedule model from form data
                FeeScheduleModel schedule = new FeeScheduleModel
                {
                    CourseID = courseID,
                    SchoolYear = schoolYear,
                    Semester = semester,
                    TuitionFeePerUnit = Convert.ToDecimal(form["TuitionFeePerUnit"]),
                    RegistrationFee = Convert.ToDecimal(form["RegistrationFee"]),
                    MiscellaneousFee = Convert.ToDecimal(form["MiscellaneousFee"]),
                    LaboratoryFee = Convert.ToDecimal(form["LaboratoryFee"])
                };

                FeeScheduleModel.InsertFeeSchedule(schedule);

                TempData["Success"] = "Fee schedule added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding fee schedule: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // GET: /FeeSchedule/Edit/5
        // Shows form to edit a fee schedule (Admin only)
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
                // Load fee schedule data by ID
                DataTable dt = FeeScheduleModel.GetFeeScheduleByID(id);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Fee schedule not found.";
                    return RedirectToAction("Index");
                }

                // Pass fee schedule data to view
                ViewBag.FeeSchedule = dt.Rows[0];

                // Load active courses for dropdown
                ViewBag.Courses = CourseModel.GetActiveCourses();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading fee schedule: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // POST: /FeeSchedule/Edit/5
        // Processes fee schedule update (Admin only)
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Edit(int id, FormCollection form)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            // Reload courses dropdown in case of error
            ViewBag.Courses = CourseModel.GetActiveCourses();

            try
            {
                int courseID = Convert.ToInt32(form["CourseID"]);
                string schoolYear = form["SchoolYear"].Trim();
                string semester = form["Semester"];

                // Check duplicate but exclude current record
                if (FeeScheduleModel.FeeScheduleExists(courseID,
                                                        schoolYear,
                                                        semester, id))
                {
                    ViewBag.Error = "A fee schedule for this course, " +
                                    "school year, and semester already exists.";
                    ViewBag.FeeSchedule = FeeScheduleModel
                                          .GetFeeScheduleByID(id).Rows[0];
                    return View();
                }

                // Build updated fee schedule model
                FeeScheduleModel schedule = new FeeScheduleModel
                {
                    FeeScheduleID = id,
                    CourseID = courseID,
                    SchoolYear = schoolYear,
                    Semester = semester,
                    TuitionFeePerUnit = Convert.ToDecimal(form["TuitionFeePerUnit"]),
                    RegistrationFee = Convert.ToDecimal(form["RegistrationFee"]),
                    MiscellaneousFee = Convert.ToDecimal(form["MiscellaneousFee"]),
                    LaboratoryFee = Convert.ToDecimal(form["LaboratoryFee"])
                };

                FeeScheduleModel.UpdateFeeSchedule(schedule);

                TempData["Success"] = "Fee schedule updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating fee schedule: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // POST: /FeeSchedule/Delete/5
        // Deletes a fee schedule (Admin only)
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
                // Check if fee schedule is used in any assessment
                object count = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM FeeAssessments WHERE FeeScheduleID = @ID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@ID", id)
                    });

                if (Convert.ToInt32(count) > 0)
                {
                    TempData["Error"] = "Cannot delete. Fee schedule is " +
                                        "already used in an assessment.";
                    return RedirectToAction("Index");
                }

                FeeScheduleModel.DeleteFeeSchedule(id);
                TempData["Success"] = "Fee schedule deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting fee schedule: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        // GET: /FeeSchedule/GetByCourseSemester
        // AJAX endpoint - returns fee schedule data as JSON
        // Called by assessment form to auto-fill fee fields
        // -------------------------------------------------------
        public JsonResult GetByCourseSemester(int courseID,
                                               string schoolYear,
                                               string semester)
        {
            try
            {
                DataTable dt = FeeScheduleModel.GetFeeScheduleByCourse(
                                courseID, schoolYear, semester);

                if (dt.Rows.Count == 0)
                    return Json(new
                    {
                        success = false,
                        message = "No fee schedule found."
                    },
                                JsonRequestBehavior.AllowGet);

                DataRow row = dt.Rows[0];

                // Return fee data as JSON for the assessment form
                return Json(new
                {
                    success = true,
                    feeScheduleID = row["FeeScheduleID"].ToString(),
                    tuitionFeePerUnit = row["TuitionFeePerUnit"].ToString(),
                    registrationFee = row["RegistrationFee"].ToString(),
                    miscellaneousFee = row["MiscellaneousFee"].ToString(),
                    laboratoryFee = row["LaboratoryFee"].ToString()
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