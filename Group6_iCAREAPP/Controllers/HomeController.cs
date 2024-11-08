using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Group6_iCAREAPP.Models;

namespace Group6_iCAREAPP.Controllers
{
    public class HomeController : Controller
    {
        private Group6_iCAREDBEntities db = new Group6_iCAREDBEntities();

        public ActionResult Index(string sortOrder)
        {
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roleID = Session["RoleID"]?.ToString();
            var roleName = Session["RoleName"]?.ToString();

            if (string.IsNullOrEmpty(roleID) || string.IsNullOrEmpty(roleName))
            {
                ViewBag.Message = "Your role is not defined. Please contact the administrator.";
                return View();
            }

            ViewBag.SortByDepartment = String.IsNullOrEmpty(sortOrder) ? "department_desc" : "";
            ViewBag.SortByPatientName = sortOrder == "PatientName" ? "name_desc" : "PatientName";
            ViewBag.SortByDOB = sortOrder == "dob" ? "dob_desc" : "dob";
            ViewBag.SortByWeight = sortOrder == "weight" ? "weight_desc" : "weight";
            ViewBag.SortByHeight = sortOrder == "height" ? "height_desc" : "height";
            ViewBag.SortByBedID = sortOrder == "bedID" ? "bedID_desc" : "bedID";
            ViewBag.SortByBloodGroup = sortOrder == "bloodGroup" ? "bloodGroup_desc" : "bloodGroup";

            List<PatientRecord> patients = db.PatientRecord.ToList();

            switch (sortOrder)
            {
                case "department_desc":
                    patients = patients.OrderByDescending(p => p.geoID).ToList();
                    break;
                case "PatientName":
                    patients = patients.OrderBy(p => p.name).ToList();
                    break;
                case "name_desc":
                    patients = patients.OrderByDescending(p => p.name).ToList();
                    break;
                case "dob":
                    patients = patients.OrderBy(p => p.dateOfBirth).ToList();
                    break;
                case "dob_desc":
                    patients = patients.OrderByDescending(p => p.dateOfBirth).ToList();
                    break;
                case "weight":
                    patients = patients.OrderBy(p => p.weight).ToList();
                    break;
                case "weight_desc":
                    patients = patients.OrderByDescending(p => p.weight).ToList();
                    break;
                case "height":
                    patients = patients.OrderBy(p => p.height).ToList();
                    break;
                case "height_desc":
                    patients = patients.OrderByDescending(p => p.height).ToList();
                    break;
                case "bedID":
                    patients = patients.OrderBy(p => p.bedID).ToList();
                    break;
                case "bedID_desc":
                    patients = patients.OrderByDescending(p => p.bedID).ToList();
                    break;
                case "bloodGroup":
                    patients = patients.OrderBy(p => p.bloodGroup).ToList();
                    break;
                case "bloodGroup_desc":
                    patients = patients.OrderByDescending(p => p.bloodGroup).ToList();
                    break;
                default:
                    patients = patients.OrderBy(p => p.geoID).ToList();
                    break;
            }

            string sql = @"
                SELECT u.ID, u.name, u.userName, u.email, r.roleName, g.description as departmentName
                FROM iCAREUser u
                LEFT JOIN UserRole r ON u.roleID = r.ID
                LEFT JOIN GeoCodes g ON u.geoID = g.geoID";

            List<LoginUserModel> users = db.Database.SqlQuery<LoginUserModel>(sql).ToList();

            HomeIndexViewModel viewModel;
            if (roleID == "1")
            {
                viewModel = new HomeIndexViewModel
                {
                    RoleID = roleID,
                    RoleName = roleName,
                    Patients = null,
                    Users = users
                };
            }
            else
            {
                viewModel = new HomeIndexViewModel
                {
                    RoleID = roleID,
                    RoleName = roleName,
                    Patients = patients,
                    Users = null
                };
            }

            return View(viewModel);
        }
    }
}
