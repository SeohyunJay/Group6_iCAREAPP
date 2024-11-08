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

        [HttpGet]
        public ActionResult Dashboard()
        {
            string sqlQuery = @"
                SELECT u.ID, u.name, u.userName, u.email, r.roleName, g.description as departmentName
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID";

            var users = db.Database.SqlQuery<LoginUserModel>(sqlQuery).ToList();

            return View(users);
        }

        [HttpGet]
        public ActionResult ManageUsers(string sortOrder)
        {
            ViewBag.SortByUsername = String.IsNullOrEmpty(sortOrder) ? "username_desc" : "Username";
            ViewBag.SortByName = sortOrder == "Name" ? "name_desc" : "Name";
            ViewBag.SortByEmail = sortOrder == "Email" ? "email_desc" : "Email";
            ViewBag.SortByRole = sortOrder == "Role" ? "role_desc" : "Role";
            ViewBag.SortByDepartment = sortOrder == "Department" ? "department_desc" : "Department";
            ViewBag.SortByContractExpirationDate = sortOrder == "ContractExpirationDate" ? "contract_exp_desc" : "ContractExpirationDate";

            string sql = @"
                SELECT u.ID, u.name, u.userName, u.email, r.roleName, g.description AS departmentName, u.contractExpirationDate
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID";

            var users = db.Database.SqlQuery<LoginUserModel>(sql).ToList();

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

            ViewBag.Roles = db.UserRole.ToList();
            ViewBag.Departments = db.GeoCodes.ToList();

            return View(users);
        }

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

            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName", user.roleID);
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description", user.geoID);

            ViewBag.ContractExpirationDate = user.contractExpirationDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.AddYears(3).ToString("yyyy-MM-dd");

            return View(user);
        }

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

                string sqlUpdateUser = @"
                    UPDATE iCAREUser
                    SET userName = @userName, name = @name, email = @email, roleID = @roleID, geoID = @GeoID, contractExpirationDate = @contractExpirationDate
                    WHERE ID = @ID";

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

            ViewBag.Roles = db.UserRole.ToList();
            ViewBag.Departments = db.GeoCodes.ToList();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(string id)
        {
            bool hasAssignedPatients = db.TreatmentRecord.Any(tr => tr.workerID == id);

            if (hasAssignedPatients)
            {
                return RedirectToAction("ManageUsers");
            }

            try
            {
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
                TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
            }

            return RedirectToAction("ManageUsers");
        }


        [HttpGet]
        public ActionResult AddUser()
        {
            var model = new iCAREUser();
            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName");
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(iCAREUser iCAREUser, string password, string selectedRoleID, string selectedGeoID)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var passwordManager = new UserPassword();
                    var encryptedPassword = passwordManager.EncryptPassword(password);

                    var newUserId = Guid.NewGuid().ToString();

                    string sqlUser = @"
                        INSERT INTO iCAREUser (ID, userName, name, email, registrationDate, roleID, geoID, contractExpirationDate)
                        VALUES (@ID, @userName, @name, @Email, @registrationDate, @roleID, @geoID, @contractExpirationDate)";

                    string sqlPassword = @"
                        INSERT INTO UserPassword (ID, userName, encryptedPassword, passwordExpiryTime, userAccountExpiryDate)
                        VALUES (@ID, @userName, @encryptedPassword, @passwordExpiryTime, @userAccountExpiryDate)";

                    int passwordExpiryTime = 90;
                    DateTime userAccountExpiryDate = DateTime.Now.AddYears(1);
                    DateTime? contractExpirationDate = iCAREUser.contractExpirationDate;

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
                    ModelState.AddModelError("", $"Error adding user: {ex.Message}");
                }
            }

            ViewBag.Roles = new SelectList(db.UserRole, "ID", "roleName");
            ViewBag.Departments = new SelectList(db.GeoCodes, "geoID", "description");

            return View(iCAREUser);
        }

        [HttpGet]
        public ActionResult AddRole()
        {
            var roles = db.UserRole.ToList();
            ViewBag.Roles = roles;
            return View(new UserRole());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRole(UserRole newRole)
        {
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

        [HttpGet]
        public ActionResult AddDepartment()
        {
            var departments = db.GeoCodes.ToList();
            ViewBag.Departments = departments;
            return View(new GeoCodes());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDepartment(GeoCodes newDepartment)
        {
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

        public ActionResult ManageRolesAndDepartments()
        {
            ViewBag.Roles = db.UserRole.ToList();
            ViewBag.Departments = db.GeoCodes.ToList();
            return View("ManageUsers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRole(string id)
        {
            string sqlDeleteRole = "DELETE FROM UserRole WHERE ID = @ID";
            db.Database.ExecuteSqlCommand(sqlDeleteRole, new SqlParameter("@ID", id));

            return RedirectToAction("AddRole");
        }

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
