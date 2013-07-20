using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebSite.Helpers;

namespace WebSite.Controllers
{
    [SessionAuthorize]
    public class InstanceController : Controller
    {
        //
        // GET: /Instance/

        public ActionResult Instance()
        {
            return View();
        }

    }
}
