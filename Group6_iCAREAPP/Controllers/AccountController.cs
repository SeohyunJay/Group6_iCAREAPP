using System;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Group6_iCAREAPP.Models;

namespace Group6_iCAREAPP.Controllers
{
    public class AccountController : Controller
    {
        // Database context for accessing database entities
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: Displays the login page
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Handles the login process and user authentication
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        public ActionResult Login(string username, string password)
        {
            // SQL query to get user and password details for authentication
            string sqlQuery = @"
                SELECT u.ID, u.name, u.userName, u.roleID, p.encryptedPassword 
                FROM iCAREUser u
                JOIN UserPassword p ON u.ID = p.ID
                WHERE u.userName = @username";

            // Execute query to find user by username
            var userRecord = db.Database.SqlQuery<LoginUserModel>(sqlQuery,
                new SqlParameter("@username", username)).FirstOrDefault();

            if (userRecord != null)
            {
                // Encrypt entered password for comparison
                var passwordManager = new UserPassword();
                string hashedEnteredPassword = passwordManager.EncryptPassword(password);

                // Check if hashed password matches stored password
                if (hashedEnteredPassword == userRecord.encryptedPassword)
                {
                    // Set user session variables
                    Session["LoggedUserID"] = userRecord.ID;
                    Session["LoggedUser"] = userRecord.userName;
                    Session["LoggedUserName"] = userRecord.name;
                    Session["RoleID"] = userRecord.roleID;
                    Session["RoleName"] = userRecord.roleName;

                    // Retrieve and store role name in session
                    string roleQuery = "SELECT roleName FROM UserRole WHERE ID = @roleID";
                    var role = db.Database.SqlQuery<string>(roleQuery,
                        new SqlParameter("@roleID", userRecord.roleID)).FirstOrDefault();

                    if (role != null)
                    {
                        Session["RoleName"] = role;
                    }

                    // Redirect based on user role (e.g., admin or regular user)
                    if (userRecord.roleID == "1")
                    {
                        return RedirectToAction("Index", "Home"); // Redirect to admin dashboard
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home"); // Redirect to user dashboard
                    }
                }
            }

            // Display error message if login fails
            ViewBag.Message = "Invalid username or password.";
            return View();
        }

        // POST: Logs the user out and clears session
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevents CSRF attacks
        public ActionResult Logout()
        {
            Session.Clear(); // Clears all session data
            return RedirectToAction("Login", "Account"); // Redirects to the login page
        }

        // GET: Displays the current user's information
        [HttpGet]
        public ActionResult MyInfo()
        {
            // Check if the user is logged in
            if (Session["LoggedUserID"] == null)
            {
                return RedirectToAction("Login"); // Redirects to login if not authenticated
            }

            // Retrieve the logged-in user's ID from session
            var userId = Session["LoggedUserID"].ToString();

            // SQL query to get user information including role and department
            string sql = @"
                SELECT u.ID, u.name, u.userName, u.email, 
                    r.roleName, 
                    g.description as departmentName
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID
                WHERE u.ID = @userId";

            // Execute query and get user info
            var userInfo = db.Database.SqlQuery<LoginUserModel>(sql, new SqlParameter("@userId", userId)).FirstOrDefault();

            // Return a 404 error if the user is not found
            if (userInfo == null)
            {
                return HttpNotFound("User not found.");
            }

            return View(userInfo); // Display the user's information
        }
    }
}
