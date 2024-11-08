using System.Web.Mvc;

public class LogoutController : Controller
{
    // POST: Handles the user logout process and clears session data
    [HttpPost]
    [ValidateAntiForgeryToken] // Prevents CSRF attacks to ensure secure logout
    public ActionResult Index()
    {
        // Clear all session data to log the user out
        Session.Clear();

        // Redirect the user to the login page after logout
        return RedirectToAction("Login", "Account");
    }
}
