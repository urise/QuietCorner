using System.Web.Mvc;
using System.Web.Routing;
using CommonClasses;

namespace WebSite.Helpers
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session == null) return;
            if (session[Constants.SESSION_AUTH_INFO] == null)
            {
                filterContext.Result = RedirectToLoginPage(filterContext.HttpContext.Request.IsAjaxRequest());
            }
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session == null) return;

            if (session[Constants.SESSION_AUTH_INFO] == null)
            {
                filterContext.Result = RedirectToLoginPage(filterContext.HttpContext.Request.IsAjaxRequest());
            }
        }

        private static ActionResult RedirectToLoginPage(bool isAjaxRequest)
        {
            SessionHelper.ClearSession();
            if (isAjaxRequest)
            {
                return new JsonResult { Data = new { Redirect = "Redirect" } };
            }
            return new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Login",
                        action = "LogOn"
                    }));
        }
    }
}