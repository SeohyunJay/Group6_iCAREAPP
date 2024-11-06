//using System;
//using System.Collections.Generic;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Net;
//using System.Web.Mvc;
//using Group6_iCAREAPP.Models;

//namespace Group6_iCAREAPP.Controllers
//{
//    public class AdminController : Controller
//    {
//        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

//        // GET: Admin/Manage
//        [HttpGet]
//        public ActionResult ManageUsers()
//        {
//            // Fetch users and join with their roles to get the role name
//            string sql = @"
//                SELECT u.ID, u.name, u.userName, u.email, r.roleName
//                FROM iCAREUser u
//                LEFT JOIN UserRole r ON u.roleID = r.ID";

//            // Fetching the data and mapping to UserWithRoleViewModel
//            var users = db.Database.SqlQuery<LoginUserModel>(sql).ToList();

//            return View(users);  // Pass the users list to the view
//        }

//        [HttpGet]
//        public ActionResult EditUser(string id) // Use 'int id' if ID is an integer
//        {
//            if (string.IsNullOrEmpty(id))
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }

//            var user = db.iCAREUser.Find(id);
//            if (user == null)
//            {
//                return HttpNotFound();
//            }

//            ViewBag.Roles = db.UserRole.ToList(); // Populate roles for dropdown
//            return View(user);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult EditUser(iCAREUser user, string selectedRoleID)
//        {
//            if (ModelState.IsValid)
//            {
//                if (string.IsNullOrEmpty(selectedRoleID))
//                {
//                    ModelState.AddModelError("selectedRoleID", "Role must be selected.");
//                    ViewBag.Roles = db.UserRole.SqlQuery("SELECT * FROM UserRole").ToList();
//                    return View(user);
//                }

//                string sqlUpdateUser = @"
//                    UPDATE iCAREUser
//                    SET userName = @userName, name = @name, email = @email, roleID = @roleID
//                    WHERE ID = @ID";

//                db.Database.ExecuteSqlCommand(
//                    sqlUpdateUser,
//                    new SqlParameter("@userName", user.userName),
//                    new SqlParameter("@name", user.name),
//                    new SqlParameter("@email", user.email),
//                    new SqlParameter("@roleID", selectedRoleID),
//                    new SqlParameter("@ID", user.ID)
//                );

//                return RedirectToAction("ManageUsers");
//            }

//            ViewBag.Roles = db.UserRole.SqlQuery("SELECT * FROM UserRole").ToList();
//            return View(user);
//        }


//        // POST: Admin/DeleteUser/{id}
//        [HttpPost]
//        public ActionResult DeleteUser(string id)
//        {
//            string sqlDeletePassword = "DELETE FROM UserPassword WHERE ID = @ID";
//            db.Database.ExecuteSqlCommand(sqlDeletePassword, new SqlParameter("@ID", id));

//            string sqlDeleteUser = "DELETE FROM iCAREUser WHERE ID = @ID";
//            db.Database.ExecuteSqlCommand(sqlDeleteUser, new SqlParameter("@ID", id));

//            return RedirectToAction("ManageUsers");
//        }

//        // GET: Admin/AddUser
//        [HttpGet]
//        public ActionResult AddUser()
//        {
//            // Fetch available roles to show in the dropdown
//            ViewBag.Roles = db.UserRole.ToList();
//            return View();
//        }

//        // POST: Admin/AddUser
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult AddUser(iCAREUser iCAREUser, string password, string selectedRoleID)
//        {
//            if (ModelState.IsValid)
//            {
//                var passwordManager = new UserPassword();
//                var encryptedPassword = passwordManager.EncryptPassword(password);

//                // Use SQL to insert the new user
//                string sqlUser = @"
//                    INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID)
//                    VALUES (@ID, @userName, @name, @Email, @registrationDate, @roleID)";

//                var newUserId = Guid.NewGuid().ToString();  // Generate unique ID

//                // SQL command to insert user password
//                string sqlPassword = @"
//                    INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
//                    VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

//                // Password and account expiry details
//                int passwordExpiryTime = 90;
//                DateTime userAccountExpiryDate = DateTime.Now.AddYears(1);

//                // Execute the SQL commands using SqlCommand
//                db.Database.ExecuteSqlCommand(
//                    sqlUser,
//                    new SqlParameter("@ID", newUserId),
//                    new SqlParameter("@userName", iCAREUser.userName),
//                    new SqlParameter("@name", iCAREUser.name),
//                    new SqlParameter("@Email", iCAREUser.email),
//                    new SqlParameter("@registrationDate", DateTime.Now),
//                    new SqlParameter("@roleID", selectedRoleID)
//                );

//                db.Database.ExecuteSqlCommand(
//                    sqlPassword,
//                    new SqlParameter("@ID", newUserId),  // Use iCAREUser.ID as the foreign key for UserPassword
//                    new SqlParameter("@userName", iCAREUser.userName),
//                    new SqlParameter("@encryptedPassword", encryptedPassword),
//                    new SqlParameter("@passwordExpiryTime", passwordExpiryTime),  // Insert password expiry time
//                    new SqlParameter("@userAccountExpiryDate", userAccountExpiryDate)  // Insert account expiry date
//                );

//                return RedirectToAction("ManageUsers");
//            }

//            // Reload roles in case of validation failure
//            ViewBag.Roles = db.UserRole.SqlQuery("SELECT * FROM UserRole").ToList();
//            return View(iCAREUser);
//        }
//    }
//}


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Group6_iCAREAPP.Models;

namespace Group6_iCAREAPP.Controllers
{
    public class AdminController : Controller
    {
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: Admin/Manage
        [HttpGet]
        public ActionResult ManageUsers()
        {
            // Fetch users along with their roles and departments (GeoID)
            string sql = @"
                SELECT u.ID, u.name, u.userName, u.email, r.roleName, g.description as departmentName
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID";

            // Fetching the data and mapping to LoginUserModel
            var users = db.Database.SqlQuery<LoginUserModel>(sql).ToList();

            return View(users);  // Pass the users list to the view
        }

        // GET: Admin/EditUser/{id}
        [HttpGet]
        public ActionResult EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = db.iCAREUser.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Populate roles and departments for dropdowns
            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName", user.roleID);
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description", user.geoID);

            return View(user);
        }

        // POST: Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(iCAREUser user, string selectedRoleID, string selectedGeoID)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(selectedRoleID))
                {
                    ModelState.AddModelError("selectedRoleID", "Role must be selected.");
                }
                if (string.IsNullOrEmpty(selectedGeoID))
                {
                    ModelState.AddModelError("selectedGeoID", "Department must be selected.");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = db.UserRole.ToList();
                    ViewBag.Departments = db.GeoCodes.ToList();
                    return View(user);
                }

                // Update user role and department
                string sqlUpdateUser = @"
                    UPDATE iCAREUser
                    SET userName = @userName, name = @name, email = @email, roleID = @roleID, geoID = @GeoID
                    WHERE ID = @ID";

                db.Database.ExecuteSqlCommand(
                    sqlUpdateUser,
                    new SqlParameter("@userName", user.userName),
                    new SqlParameter("@name", user.name),
                    new SqlParameter("@email", user.email),
                    new SqlParameter("@roleID", selectedRoleID),
                    new SqlParameter("@GeoID", selectedGeoID),
                    new SqlParameter("@ID", user.ID)
                );

                return RedirectToAction("ManageUsers");
            }

            // Reload roles and departments in case of validation failure
            ViewBag.Roles = db.UserRole.ToList();
            ViewBag.Departments = db.GeoCodes.ToList();
            return View(user);
        }

        // POST: Admin/DeleteUser/{id}
        [HttpPost]
        public ActionResult DeleteUser(string id)
        {
            string sqlDeletePassword = "DELETE FROM UserPassword WHERE ID = @ID";
            db.Database.ExecuteSqlCommand(sqlDeletePassword, new SqlParameter("@ID", id));

            string sqlDeleteUser = "DELETE FROM iCAREUser WHERE ID = @ID";
            db.Database.ExecuteSqlCommand(sqlDeleteUser, new SqlParameter("@ID", id));

            return RedirectToAction("ManageUsers");
        }

        // GET: Admin/AddUser
        //[HttpGet]
        //public ActionResult AddUser()
        //{
        //    // Fetch available roles and departments for dropdowns
        //    ViewBag.Roles = db.UserRole.ToList();
        //    ViewBag.Departments = db.GeoCodes.ToList();
        //    return View();
        //}

        //// POST: Admin/AddUser
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult AddUser(iCAREUser iCAREUser, string password, string selectedRoleID, string selectedGeoID)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (string.IsNullOrEmpty(selectedRoleID))
        //        {
        //            ModelState.AddModelError("selectedRoleID", "Role must be selected.");
        //        }
        //        if (string.IsNullOrEmpty(selectedGeoID))
        //        {
        //            ModelState.AddModelError("selectedGeoID", "Department must be selected.");
        //        }

        //        if (!ModelState.IsValid)
        //        {
        //            ViewBag.Roles = db.UserRole.ToList();
        //            ViewBag.Departments = db.GeoCodes.ToList();
        //            return View(iCAREUser);
        //        }

        //        var passwordManager = new UserPassword();
        //        var encryptedPassword = passwordManager.EncryptPassword(password);

        //        // Use SQL to insert the new user
        //        string sqlUser = @"
        //            INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID, geoID)
        //            VALUES (@ID, @userName, @name, @Email, @registrationDate, @roleID, @GeoID)";

        //        var newUserId = Guid.NewGuid().ToString();  // Generate unique ID

        //        // SQL command to insert user password
        //        string sqlPassword = @"
        //            INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
        //            VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

        //        // Password and account expiry details
        //        int passwordExpiryTime = 90;
        //        DateTime userAccountExpiryDate = DateTime.Now.AddYears(1);

        //        // Execute the SQL commands using SqlCommand
        //        db.Database.ExecuteSqlCommand(
        //            sqlUser,
        //            new SqlParameter("@ID", newUserId),
        //            new SqlParameter("@userName", iCAREUser.userName),
        //            new SqlParameter("@name", iCAREUser.name),
        //            new SqlParameter("@Email", iCAREUser.email),
        //            new SqlParameter("@registrationDate", DateTime.Now),
        //            new SqlParameter("@roleID", selectedRoleID),
        //            new SqlParameter("@GeoID", selectedGeoID)
        //        );

        //        db.Database.ExecuteSqlCommand(
        //            sqlPassword,
        //            new SqlParameter("@ID", newUserId),
        //            new SqlParameter("@userName", iCAREUser.userName),
        //            new SqlParameter("@encryptedPassword", encryptedPassword),
        //            new SqlParameter("@passwordExpiryTime", passwordExpiryTime),
        //            new SqlParameter("@userAccountExpiryDate", userAccountExpiryDate)
        //        );

        //        return RedirectToAction("ManageUsers");
        //    }

        //    // Reload roles and departments in case of validation failure
        //    ViewBag.Roles = db.UserRole.ToList();
        //    ViewBag.Departments = db.GeoCodes.ToList();
        //    return View(iCAREUser);
        //}
        [HttpGet]
        public ActionResult AddUser()
        {
            // Fetch available roles for the dropdown
            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName");
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description"); // Assuming geoID is used as the department
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(iCAREUser iCAREUser, string password, string selectedRoleID, string selectedGeoID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Encrypt the password
                    var passwordManager = new UserPassword();
                    var encryptedPassword = passwordManager.EncryptPassword(password);

                    // Generate a new unique ID for the user
                    var newUserId = Guid.NewGuid().ToString();

                    // SQL command to insert the user into the iCAREUser table
                    string sqlUser = @"
                INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID, geoID)
                VALUES (@ID, @userName, @name, @Email, @registrationDate, @roleID, @geoID)";

                    // SQL command to insert user password into UserPassword table
                    string sqlPassword = @"
                INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
                VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

                    // Define password and account expiry dates
                    int passwordExpiryTime = 90; // Expiry in 90 days
                    DateTime userAccountExpiryDate = DateTime.Now.AddYears(1); // Account expiry in one year

                    // Insert user details
                    db.Database.ExecuteSqlCommand(
                        sqlUser,
                        new SqlParameter("@ID", newUserId),
                        new SqlParameter("@userName", iCAREUser.userName),
                        new SqlParameter("@name", iCAREUser.name),
                        new SqlParameter("@Email", iCAREUser.email),
                        new SqlParameter("@registrationDate", DateTime.Now),
                        new SqlParameter("@roleID", selectedRoleID),
                        new SqlParameter("@geoID", selectedGeoID) // Department (geoID)
                    );

                    // Insert password details
                    db.Database.ExecuteSqlCommand(
                        sqlPassword,
                        new SqlParameter("@ID", newUserId),
                        new SqlParameter("@userName", iCAREUser.userName),
                        new SqlParameter("@encryptedPassword", encryptedPassword),
                        new SqlParameter("@passwordExpiryTime", passwordExpiryTime),
                        new SqlParameter("@userAccountExpiryDate", userAccountExpiryDate)
                    );

                    TempData["SuccessMessage"] = "User added successfully.";
                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error adding user: {ex.Message}");
                }
            }

            // Reload roles and departments in case of validation failure
            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName");
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description");

            return View(iCAREUser);
        }

    }
}
