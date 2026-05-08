using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class StudentController : Controller
    {
        // -------------------------------------------------------
        // GET: /Student/Index
        // Shows list of all students (Admin only)
        // -------------------------------------------------------
        public ActionResult Index()
        {
            // Only logged in users can access this
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            // Only admin can see all students
            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            try
            {
                // Get all students with their course name
                DataTable dt = StudentModel.GetAllStudents();
                ViewBag.Students = dt;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading students: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // GET: /Student/Create
        // Shows form to register a new student (Admin only)
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

            // Auto-generate student number
            ViewBag.StudentNumber = StudentModel.GenerateStudentNumber();

            return View();
        }

        // -------------------------------------------------------
        // POST: /Student/Create
        // Processes new student registration (Admin only)
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
                string username = form["Username"].Trim();
                string password = form["Password"].Trim();

                // Check if username already exists
                if (UserModel.UsernameExists(username))
                {
                    ViewBag.Error = "Username already exists. Choose another.";
                    return View();
                }

                // Check if student number already exists
                string studentNumber = form["StudentNumber"].Trim();
                if (StudentModel.StudentNumberExists(studentNumber))
                {
                    ViewBag.Error = "Student number already exists.";
                    return View();
                }

                // Step 1: Create the User account first
                int rowsAffected = UserModel.InsertUser(username, password, "Student");

                if (rowsAffected == 0)
                {
                    ViewBag.Error = "Failed to create user account.";
                    return View();
                }

                // Step 2: Get the newly created UserID
                DataTable userDT = UserModel.GetUserByUsername(username);
                int newUserID = Convert.ToInt32(userDT.Rows[0]["UserID"]);

                // Step 3: Create the Student record linked to the User
                StudentModel student = new StudentModel
                {
                    UserID = newUserID,
                    StudentNumber = studentNumber,
                    FirstName = form["FirstName"].Trim(),
                    MiddleName = form["MiddleName"].Trim(),
                    LastName = form["LastName"].Trim(),
                    Gender = form["Gender"],
                    BirthDate = Convert.ToDateTime(form["BirthDate"]),
                    Address = form["Address"].Trim(),
                    ContactNumber = form["ContactNumber"].Trim(),
                    Email = form["Email"].Trim(),
                    CourseID = Convert.ToInt32(form["CourseID"]),
                    YearLevel = Convert.ToInt32(form["YearLevel"]),
                    Semester = form["Semester"],
                    SchoolYear = form["SchoolYear"].Trim(),
                    EnrollmentStatus = "Enrolled"
                };

                StudentModel.InsertStudent(student);

                // Redirect to student list with success message
                TempData["Success"] = "Student registered successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error registering student: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // GET: /Student/Edit/5
        // Shows form to edit a student record (Admin only)
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
                // Load student data by ID
                DataTable dt = StudentModel.GetStudentByID(id);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                // Pass student data to view
                ViewBag.Student = dt.Rows[0];

                // Load active courses for dropdown
                ViewBag.Courses = CourseModel.GetActiveCourses();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading student: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // POST: /Student/Edit/5
        // Processes student record update (Admin only)
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Edit(int id, FormCollection form)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (!AccountController.IsAdmin())
                return RedirectToAction("Index", "Dashboard");

            ViewBag.Courses = CourseModel.GetActiveCourses();

            try
            {
                // Build updated student model
                StudentModel student = new StudentModel
                {
                    StudentID = id,
                    FirstName = form["FirstName"].Trim(),
                    MiddleName = form["MiddleName"].Trim(),
                    LastName = form["LastName"].Trim(),
                    Gender = form["Gender"],
                    BirthDate = Convert.ToDateTime(form["BirthDate"]),
                    Address = form["Address"].Trim(),
                    ContactNumber = form["ContactNumber"].Trim(),
                    Email = form["Email"].Trim(),
                    CourseID = Convert.ToInt32(form["CourseID"]),
                    YearLevel = Convert.ToInt32(form["YearLevel"]),
                    Semester = form["Semester"],
                    SchoolYear = form["SchoolYear"].Trim(),
                    EnrollmentStatus = form["EnrollmentStatus"]
                };

                StudentModel.UpdateStudent(student);

                TempData["Success"] = "Student updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating student: " + ex.Message;
                return View();
            }
        }

        // -------------------------------------------------------
        // GET: /Student/Details/5
        // Shows full student profile and assessment history
        // -------------------------------------------------------
        public ActionResult Details(int id)
        {
            if (!AccountController.IsLoggedIn())
                return RedirectToAction("Login", "Account");

            try
            {
                // Load student info
                DataTable studentDT = StudentModel.GetStudentByID(id);

                if (studentDT.Rows.Count == 0)
                {
                    TempData["Error"] = "Student not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.Student = studentDT.Rows[0];

                // Load student's assessment history
                DataTable assessments = FeeAssessmentModel
                                        .GetAssessmentsByStudent(id);
                ViewBag.Assessments = assessments;

                // Load student's payment history
                DataTable payments = PaymentModel.GetPaymentsByStudent(id);
                ViewBag.Payments = payments;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading student details: " + ex.Message;
            }

            return View();
        }

        // -------------------------------------------------------
        // POST: /Student/Delete/5
        // Deletes a student record (Admin only)
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
                // Step 1: Get all assessments for this student
                System.Data.DataTable assessments =
                    FeeAssessmentModel.GetAssessmentsByStudent(id);

                foreach (System.Data.DataRow assessment in assessments.Rows)
                {
                    int assessmentID = Convert.ToInt32(assessment["AssessmentID"]);

                    // Step 2: Delete payments linked to this assessment
                    DBHelper.ExecuteNonQuery(
                        "DELETE FROM Payments WHERE AssessmentID = @AssessmentID",
                        new System.Data.SqlClient.SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter(
                        "@AssessmentID", assessmentID)
                        });

                    // Step 3: Delete payment terms linked to this assessment
                    DBHelper.ExecuteNonQuery(
                        "DELETE FROM PaymentTerms WHERE AssessmentID = @AssessmentID",
                        new System.Data.SqlClient.SqlParameter[] {
                    new System.Data.SqlClient.SqlParameter(
                        "@AssessmentID", assessmentID)
                        });
                }

                // Step 4: Delete all assessments for this student
                DBHelper.ExecuteNonQuery(
                    "DELETE FROM FeeAssessments WHERE StudentID = @StudentID",
                    new System.Data.SqlClient.SqlParameter[] {
                new System.Data.SqlClient.SqlParameter("@StudentID", id)
                    });

                // Step 5: Get UserID before deleting student
                System.Data.DataTable studentDT = StudentModel.GetStudentByID(id);
                int userID = 0;
                if (studentDT.Rows.Count > 0)
                    userID = Convert.ToInt32(studentDT.Rows[0]["UserID"]);

                // Step 6: Delete the student record
                StudentModel.DeleteStudent(id);

                // Step 7: Delete the linked user account
                if (userID > 0)
                    UserModel.DeleteUser(userID);

                TempData["Success"] = "Student and all related records deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting student: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}