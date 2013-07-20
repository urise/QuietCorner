using System.Web.Mvc;
using System.Web.Routing;
using CommonClasses;

namespace WebSite.Helpers
{
    public class PermissionsAttribute : ActionFilterAttribute
    {
        private readonly AccessLevel _access;
        private readonly AccessComponent _component;

        public PermissionsAttribute(AccessComponent component, AccessLevel access)
        {
            _component = component;
            _access = access;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            if (session == null) return;

            if (SessionHelper.Permissions == null || !SessionHelper.Permissions.IsGranted(_component, _access))
            {
                filterContext.Result = NotEnoughPermissionsResult(filterContext.HttpContext.Request.IsAjaxRequest());
            }
        }

        private static ActionResult NotEnoughPermissionsResult(bool isAjaxRequest)
        {
            if (isAjaxRequest)
            {
                return new JsonResult { Data = new { ErrorMessage = Messages.NotEnoughPermissions } };
            }
            return new RedirectToRouteResult(
                    new RouteValueDictionary(
                        new
                        {
                            controller = "Error",
                            action = "AccessError"
                        }));
        }
    }
}