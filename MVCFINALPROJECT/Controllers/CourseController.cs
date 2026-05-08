using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class CourseController : Controller
    {
        // -------------------------------------------------------
        // GET: /Course/Index
        // Shows list of all courses (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Get all courses including inactive ones
                DataTable dt = CourseModel.GetAllCourses();
                ViewBag.Courses = dt;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading courses: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Course/Create
        // Shows form to add a new course (Admin only)
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
        // POST: /Course/Create
        // Processes new course creation (Admin only)
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
                string courseCode = form["CourseCode"].Trim();

                // Check if course code already exists
                if (CourseModel.CourseCodeExists(courseCode))
                {
                    ViewBag.Error = "Course code already exists.";
                    return View();
                }

                // Build new course model from form data
                CourseModel course = new CourseModel
                {
                    CourseCode = courseCode,
                    CourseName = form["CourseName"].Trim(),
                    Department = form["Department"].Trim(),
                    // Checkbox returns "true" if checked
                    IsActive = form["IsActive"] == "true"
                };

                CourseModel.InsertCourse(course);

                TempData["Success"] = "Course added successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding course: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // GET: /Course/Edit/5
        // Shows form to edit a course (Admin only)
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
                // Load course data by ID
                DataTable dt = CourseModel.GetCourseByID(id);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Course not found.";
                    return RedirectToAction("Index");
                }

                // Pass course data to view
                ViewBag.Course = dt.Rows[0];
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading course: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // POST: /Course/Edit/5
        // Processes course update (Admin only)
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
                string courseCode = form["CourseCode"].Trim();

                // Check if course code exists but exclude current record
                if (CourseModel.CourseCodeExists(courseCode, id))
                {
                    ViewBag.Error = "Course code already exists.";
                    // Reload course data for the form
                    ViewBag.Course = CourseModel.GetCourseByID(id).Rows[0];
                    return View();
                }

                // Build updated course model
                CourseModel course = new CourseModel
                {
                    CourseID = id,
                    CourseCode = courseCode,
                    CourseName = form["CourseName"].Trim(),
                    Department = form["Department"].Trim(),
                    IsActive = form["IsActive"] == "true"
                };

                CourseModel.UpdateCourse(course);

                TempData["Success"] = "Course updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating course: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // POST: /Course/Delete/5
        // Deletes a course (Admin only)
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
                // Check if course has students before deleting
                object count = DBHelper.ExecuteScalar(
                    "SELECT COUNT(*) FROM Students WHERE CourseID = @CourseID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@CourseID", id)
                    });

                if (Convert.ToInt32(count) > 0)
                {
                    TempData["Error"] = "Cannot delete course because students " +
                                        "are enrolled in it. Use the Toggle button " +
                                        "to deactivate it instead.";
                    return RedirectToAction("Index");
                }

                CourseModel.DeleteCourse(id);
                TempData["Success"] = "Course deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting course: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // -------------------------------------------------------
        // POST: /Course/ToggleStatus/5
        // Activates or deactivates a course instead of deleting
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
                    @"UPDATE Courses 
                      SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                      WHERE CourseID = @CourseID",
                    new System.Data.SqlClient.SqlParameter[] {
                        new System.Data.SqlClient.SqlParameter("@CourseID", id)
                    });

                TempData["Success"] = "Course status updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating status: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}