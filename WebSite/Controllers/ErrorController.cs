using System.Web.Mvc;

namespace WebSite.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult AccessError()
        {
            return View();
        }
    }
}
