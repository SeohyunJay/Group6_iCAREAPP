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
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            string sqlQuery = @"
                SELECT u.ID, u.name, u.userName, u.roleID, p.encryptedPassword 
                FROM iCAREUser u
                JOIN UserPassword p ON u.ID = p.ID
                WHERE u.userName = @username";

            var userRecord = db.Database.SqlQuery<LoginUserModel>(sqlQuery,
                new SqlParameter("@username", username)).FirstOrDefault();

            if (userRecord != null)
            {
                var passwordManager = new UserPassword();
                string hashedEnteredPassword = passwordManager.EncryptPassword(password);

                if (hashedEnteredPassword == userRecord.encryptedPassword)
                {
                    Session["LoggedUserID"] = userRecord.ID;
                    Session["LoggedUser"] = userRecord.userName;
                    Session["LoggedUserName"] = userRecord.name;
                    Session["RoleID"] = userRecord.roleID;
                    Session["RoleName"] = userRecord.roleName;

                    string roleQuery = "SELECT roleName FROM UserRole WHERE ID = @roleID";
                    var role = db.Database.SqlQuery<string>(roleQuery,
                        new SqlParameter("@roleID", userRecord.roleID)).FirstOrDefault();

                    if (role != null)
                    {
                        Session["RoleName"] = role;
                    }

                    if (userRecord.roleID == "1")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.Message = "Invalid username or password.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult MyInfo()
        {
            if (Session["LoggedUserID"] == null)
            {
                return RedirectToAction("Login");
            }

            var userId = Session["LoggedUserID"].ToString();

            string sql = @"
                SELECT u.ID, u.name, u.userName, u.email, 
                    r.roleName, 
                    g.description as departmentName
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID
                WHERE u.ID = @userId";

            var userInfo = db.Database.SqlQuery<LoginUserModel>(sql, new SqlParameter("@userId", userId)).FirstOrDefault();

            if (userInfo == null)
            {
                return HttpNotFound("User not found.");
            }

            return View(userInfo);
        }
    }
}
