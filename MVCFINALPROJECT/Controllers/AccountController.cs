using System;
using System.Data;
using System.Web.Mvc;
using MVCFINALPROJECT.Models;

namespace MVCFINALPROJECT.Controllers
{
    public class AccountController : Controller
    {
        // -------------------------------------------------------
        // GET: /Account/Login
        // Shows the login page
        // -------------------------------------------------------
        [HttpGet]
        public ActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (Session["UserID"] != null)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // -------------------------------------------------------
        // POST: /Account/Login
        // Processes login form submission
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            // Validate input fields are not empty
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            // Look up user by username in the database
            DataTable dt = UserModel.GetUserByUsername(username);

            if (dt.Rows.Count == 0)
            {
                // Username not found
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            DataRow user = dt.Rows[0];

            // Check if account is active
            if (!Convert.ToBoolean(user["IsActive"]))
            {
                ViewBag.Error = "Your account has been deactivated. Contact admin.";
                return View();
            }

            // Check password (plain text for now)
            // TODO: use hashed password comparison in production
            if (user["PasswordHash"].ToString() != password)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            // Store user info in Session
            Session["UserID"] = user["UserID"].ToString();
            Session["Username"] = user["Username"].ToString();
            Session["Role"] = user["Role"].ToString();

            // If student, also store their StudentID in session
            if (user["Role"].ToString() == "Student")
            {
                DataTable studentDT = StudentModel.GetStudentByUserID(
                                        Convert.ToInt32(user["UserID"]));

                if (studentDT.Rows.Count > 0)
                {
                    Session["StudentID"] = studentDT.Rows[0]["StudentID"].ToString();
                    Session["StudentName"] = studentDT.Rows[0]["FirstName"].ToString()
                                          + " "
                                          + studentDT.Rows[0]["LastName"].ToString();
                }
            }

            // Redirect to Dashboard after successful login
            return RedirectToAction("Index", "Dashboard");
        }

        // -------------------------------------------------------
        // GET: /Account/Logout
        // Clears session and redirects to login
        // -------------------------------------------------------
        public ActionResult Logout()
        {
            // Clear all session data
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        // -------------------------------------------------------
        // GET: /Account/ChangePassword
        // Shows change password form (for logged in users)
        // -------------------------------------------------------
        [HttpGet]
        public ActionResult ChangePassword()
        {
            // Must be logged in to change password
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // -------------------------------------------------------
        // POST: /Account/ChangePassword
        // Processes password change
        // -------------------------------------------------------
        [HttpPost]
        public ActionResult ChangePassword(string currentPassword,
                                           string newPassword,
                                           string confirmPassword)
        {
            if (Session["UserID"] == null)
                return RedirectToAction("Login", "Account");

            int userID = Convert.ToInt32(Session["UserID"]);

            // Get current user record
            DataTable dt = UserModel.GetUserByID(userID);
            DataRow user = dt.Rows[0];

            // Verify current password
            if (user["PasswordHash"].ToString() != currentPassword)
            {
                ViewBag.Error = "Current password is incorrect.";
                return View();
            }

            // Check new password and confirm match
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "New password and confirm password do not match.";
                return View();
            }

            // Check new password length
            if (newPassword.Length < 6)
            {
                ViewBag.Error = "New password must be at least 6 characters.";
                return View();
            }

            // Update password in database
            string query = "UPDATE Users SET PasswordHash = @Password WHERE UserID = @UserID";
            System.Data.SqlClient.SqlParameter[] param = {
                new System.Data.SqlClient.SqlParameter("@Password", newPassword),
                new System.Data.SqlClient.SqlParameter("@UserID", userID)
            };
            DBHelper.ExecuteNonQuery(query, param);

            ViewBag.Success = "Password changed successfully.";
            return View();
        }

        // -------------------------------------------------------
        // Helper: Check if current user is Admin
        // Used by other controllers to restrict access
        // -------------------------------------------------------
        public static bool IsAdmin()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            return context.Session["Role"] != null &&
                   context.Session["Role"].ToString() == "Admin";
        }

        // -------------------------------------------------------
        // Helper: Check if user is logged in
        // -------------------------------------------------------
        public static bool IsLoggedIn()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            return context.Session["UserID"] != null;
        }
    }
}