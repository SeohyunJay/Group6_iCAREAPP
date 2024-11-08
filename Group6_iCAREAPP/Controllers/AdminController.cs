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
        // Database context for accessing database entities
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        // GET: Displays the admin dashboard with a list of users
        [HttpGet]
        public ActionResult Dashboard()
        {
            // SQL query to retrieve user details and related role/department information
            string sqlQuery = @"
                SELECT u.ID, u.name, u.userName, u.email, r.roleName, g.description as departmentName
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID";

            // Execute the query and return the result as a list
            var users = db.Database.SqlQuery<LoginUserModel>(sqlQuery).ToList();

            return View(users);
        }

        // GET: Displays the Manage Users page with sorting options
        [HttpGet]
        public ActionResult ManageUsers(string sortOrder)
        {
            // Configure sort options for user properties
            ViewBag.SortByUsername = String.IsNullOrEmpty(sortOrder) ? "username_desc" : "Username";
            ViewBag.SortByName = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.SortByEmail = sortOrder == "Email" ? "email_desc" : "Email";
            ViewBag.SortByRole = sortOrder == "Role" ? "role_desc" : "Role";
            ViewBag.SortByDepartment = sortOrder == "Department" ? "department_desc" : "Department";
            ViewBag.SortByContractExpirationDate = sortOrder == "ContractExpirationDate" ? "contract_exp_desc" : "ContractExpirationDate";

            // SQL query to get user details including role and department information
            string sql = @"
                SELECT u.ID, u.name, u.userName, u.email, r.roleName, g.description AS departmentName, u.contractExpirationDate
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID";

            // Execute the query and sort results based on the sort order
            var users = db.Database.SqlQuery<LoginUserModel>(sql).ToList();

            // Sorting logic based on selected sort order
            switch (sortOrder)
            {
                case "username_desc":
                    users = users.OrderByDescending(u => u.userName).ToList();
                    break;
                case "Name":
                    users = users.OrderBy(u => u.name).ToList();
                    break;
                case "name_desc":
                    users = users.OrderByDescending(u => u.name).ToList();
                    break;
                case "Email":
                    users = users.OrderBy(u => u.email).ToList();
                    break;
                case "email_desc":
                    users = users.OrderByDescending(u => u.email).ToList();
                    break;
                case "Role":
                    users = users.OrderBy(u => u.roleName).ToList();
                    break;
                case "role_desc":
                    users = users.OrderByDescending(u => u.roleName).ToList();
                    break;
                case "Department":
                    users = users.OrderBy(u => u.departmentName).ToList();
                    break;
                case "department_desc":
                    users = users.OrderByDescending(u => u.departmentName).ToList();
                    break;
                case "ContractExpirationDate":
                    users = users.OrderBy(u => u.contractExpirationDate).ToList();
                    break;
                case "contract_exp_desc":
                    users = users.OrderByDescending(u => u.contractExpirationDate).ToList();
                    break;
                default:
                    users = users.OrderBy(u => u.roleName).ToList();
                    break;
            }

            // Load roles and departments for the view
            ViewBag.Roles = db.UserRole.ToList();
            ViewBag.Departments = db.GeoCodes.ToList();

            return View(users);
        }

        // GET: Displays the form to edit a user's details
        [HttpGet]
        public ActionResult EditUser(string id)
        {
            // Check if ID is provided
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Retrieve the user from the database
            var user = db.iCAREUser.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Set ViewBag properties for dropdown lists
            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName", user.roleID);
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description", user.geoID);

            // Display contract expiration date
            ViewBag.ContractExpirationDate = user.contractExpirationDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddYears(3).ToString("yyyy-MM-dd");

            return View(user);
        }

        // POST: Saves changes to the edited user
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(iCAREUser user, string selectedRoleID, string selectedGeoID)
        {
            // Validate model and check if role and department are selected
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

                // Return the view with errors if validation fails
                if (!ModelState.IsValid)
                {
                    ViewBag.Roles = db.UserRole.ToList();
                    ViewBag.Departments = db.GeoCodes.ToList();
                    return View(user);
                }

                // SQL command to update user details
                string sqlUpdateUser = @"
                    UPDATE iCAREUser
                    SET userName = @userName, name = @name, email = @email, roleID = @roleID, geoID = @GeoID, contractExpirationDate = @contractExpirationDate
                    WHERE ID = @ID";

                // Execute the update command
                db.Database.ExecuteSqlCommand(
                    sqlUpdateUser,
                    new SqlParameter("@userName", user.userName),
                    new SqlParameter("@name", user.name),
                    new SqlParameter("@email", user.email),
                    new SqlParameter("@roleID", selectedRoleID),
                    new SqlParameter("@GeoID", selectedGeoID),
                    new SqlParameter("@contractExpirationDate", user.contractExpirationDate),
                    new SqlParameter("@ID", user.ID)
                );

                return RedirectToAction("ManageUsers");
            }

            // Reload roles and departments for the view in case of errors
            ViewBag.Roles = db.UserRole.ToList();
            ViewBag.Departments = db.GeoCodes.ToList();
            return View(user);
        }

        // POST: Deletes a user from the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string id)
        {
            // Check if the user has assigned patients to prevent deletion
            bool hasAssignedPatients = db.TreatmentRecord.Any(tr => tr.workerID == id);

            if (hasAssignedPatients)
            {
                return RedirectToAction("ManageUsers");
            }

            try
            {
                // SQL commands to delete related data before deleting the user
                string deleteModificationHistorySql = "DELETE FROM ModificationHistory WHERE modID IN (SELECT modID FROM DocumentMetadata WHERE createdByID = @ID OR modifiedByID = @ID)";
                db.Database.ExecuteSqlCommand(deleteModificationHistorySql, new SqlParameter("@ID", id));

                string deleteDocumentsSql = "DELETE FROM DocumentMetadata WHERE createdByID = @ID OR modifiedByID = @ID";
                db.Database.ExecuteSqlCommand(deleteDocumentsSql, new SqlParameter("@ID", id));

                string deleteUserPasswordSql = "DELETE FROM UserPassword WHERE ID = @ID";
                db.Database.ExecuteSqlCommand(deleteUserPasswordSql, new SqlParameter("@ID", id));

                string deleteUserSql = "DELETE FROM iCAREUser WHERE ID = @ID";
                db.Database.ExecuteSqlCommand(deleteUserSql, new SqlParameter("@ID", id));
            }
            catch (Exception ex)
            {
                // Display an error message if deletion fails
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction("ManageUsers");
        }

        // GET: Displays the form for adding a new user
        [HttpGet]
        public ActionResult AddUser()
        {
            var model = new iCAREUser();
            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName");
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description");
            return View(model);
        }

        // POST: Adds a new user to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(iCAREUser iCAREUser, string password, string selectedRoleID, string selectedGeoID)
        {
            // Validate model before adding a new user
            if (ModelState.IsValid)
            {
                try
                {
                    // Encrypt password and generate new user ID
                    var passwordManager = new UserPassword();
                    var encryptedPassword = passwordManager.EncryptPassword(password);
                    var newUserId = Guid.NewGuid().ToString();

                    // SQL commands to insert new user and password data
                    string sqlUser = @"
                        INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID, geoID, contractExpirationDate)
                        VALUES (@ID, @userName, @name, @Email, @registrationDate, @roleID, @geoID, @contractExpirationDate)";

                    string sqlPassword = @"
                        INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
                        VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

                    int passwordExpiryTime = 90; // Password expiry in days
                    DateTime userAccountExpiryDate = DateTime.Now.AddYears(1);
                    DateTime? contractExpirationDate = iCAREUser.contractExpirationDate;

                    // Execute SQL commands for user and password
                    db.Database.ExecuteSqlCommand(
                        sqlUser,
                        new SqlParameter("@ID", newUserId),
                        new SqlParameter("@userName", iCAREUser.userName),
                        new SqlParameter("@name", iCAREUser.name),
                        new SqlParameter("@Email", iCAREUser.email),
                        new SqlParameter("@registrationDate", DateTime.Now),
                        new SqlParameter("@roleID", selectedRoleID),
                        new SqlParameter("@geoID", selectedGeoID),
                        new SqlParameter("@contractExpirationDate", contractExpirationDate)
                    );

                    db.Database.ExecuteSqlCommand(
                        sqlPassword,
                        new SqlParameter("@ID", newUserId),
                        new SqlParameter("@userName", iCAREUser.userName),
                        new SqlParameter("@encryptedPassword", encryptedPassword),
                        new SqlParameter("@passwordExpiryTime", passwordExpiryTime),
                        new SqlParameter("@userAccountExpiryDate", userAccountExpiryDate)
                    );

                    return RedirectToAction("ManageUsers");
                }
                catch (Exception ex)
                {
                    // Handle errors during user addition
                    ModelState.AddModelError("", $"Error adding user: {ex.Message}");
                }
            }

            // Reload roles and departments for the view in case of errors
            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName");
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description");

            return View(iCAREUser);
        }

        // GET: Displays the form to add a new role
        [HttpGet]
        public ActionResult AddRole()
        {
            var roles = db.UserRole.ToList();
            ViewBag.Roles = roles;
            return View(new UserRole());
        }

        // POST: Adds a new role to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRole(UserRole newRole)
        {
            // Validate and insert new role
            if (ModelState.IsValid)
            {
                string sqlInsertRole = "INSERT INTO UserRole (ID, roleName) VALUES (@ID, @roleName)";
                var newRoleId = Guid.NewGuid().ToString();

                db.Database.ExecuteSqlCommand(
                    sqlInsertRole,
                    new SqlParameter("@ID", newRoleId),
                    new SqlParameter("@roleName", newRole.roleName)
                );

                return RedirectToAction("AddRole");
            }

            return View(newRole);
        }

        // GET: Displays the form to add a new department
        [HttpGet]
        public ActionResult AddDepartment()
        {
            var departments = db.GeoCodes.ToList();
            ViewBag.Departments = departments;
            return View(new GeoCodes());
        }

        // POST: Adds a new department to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDepartment(GeoCodes newDepartment)
        {
            // Validate and insert new department
            if (ModelState.IsValid)
            {
                string sqlInsertDepartment = "INSERT INTO GeoCodes (geoID, description) VALUES (@geoID, @description)";
                var newDepartmentId = Guid.NewGuid().ToString();

                db.Database.ExecuteSqlCommand(
                    sqlInsertDepartment,
                    new SqlParameter("@geoID", newDepartmentId),
                    new SqlParameter("@description", newDepartment.description)
                );

                return RedirectToAction("AddDepartment");
            }

            return View(newDepartment);
        }

        // GET: Manages roles and departments
        public ActionResult ManageRolesAndDepartments()
        {
            ViewBag.Roles = db.UserRole.ToList();
            ViewBag.Departments = db.GeoCodes.ToList();
            return View("ManageUsers");
        }

        // POST: Deletes a role from the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRole(string id)
        {
            string sqlDeleteRole = "DELETE FROM UserRole WHERE ID = @ID";
            db.Database.ExecuteSqlCommand(sqlDeleteRole, new SqlParameter("@ID", id));

            return RedirectToAction("AddRole");
        }

        // POST: Deletes a department from the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDepartment(string id)
        {
            string sqlDeleteDepartment = "DELETE FROM GeoCodes WHERE geoID = @geoID";
            db.Database.ExecuteSqlCommand(sqlDeleteDepartment, new SqlParameter("@geoID", id));

            return RedirectToAction("AddDepartment");
        }
    }
}
