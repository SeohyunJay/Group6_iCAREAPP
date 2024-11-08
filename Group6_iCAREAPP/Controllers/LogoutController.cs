using System.Web.Mvc;
public class LogoutController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Index()
    {
        Session.Clear();

        return RedirectToAction("Login", "Account");
    }
}
