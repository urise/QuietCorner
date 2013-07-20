using System.Web.Mvc;
using WebSite.Helpers;

namespace WebSite.Controllers
{
    [SessionAuthorize]
    public class HomeController : Controller
    {
        public ActionResult Home()
        {
            return View();
        }
    }
}