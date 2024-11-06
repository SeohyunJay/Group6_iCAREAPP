using System.Web.Mvc;

namespace Group6_iCAREAPP.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            // Check if the user is logged in
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve role details from session
            var roleID = Session["RoleID"]?.ToString();
            var roleName = Session["RoleName"]?.ToString();

            // If no role is defined
            if (string.IsNullOrEmpty(roleID) || string.IsNullOrEmpty(roleName))
            {
                ViewBag.Message = "Your role is not defined. Please contact the administrator.";
                return View();
            }

            // Pass role information to the view
            ViewBag.RoleID = roleID;
            ViewBag.RoleName = roleName;

            return View(); // Render the Index.cshtml view for all roles
        }
    }
}
