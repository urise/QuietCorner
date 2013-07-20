using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.MethodArguments;
using CommonClasses.Models;
using Interfaces.Enums;
using ServiceProxy;
using WebSite.Helpers;

namespace WebSite.Controllers
{
    [SessionAuthorize]
    public class RolesController : Controller
    {
        #region  Properties and additional methods
        protected List<RoleModel> StoredCompanyRoles
        {
            get { return Session[Constants.SESSION_INSTANCE_ROLES] as List<RoleModel>; }
            set { Session[Constants.SESSION_INSTANCE_ROLES] = value; }
        }

        private List<RoleModel> GetCompanyRoles(string searchString)
        {
            var list = StoredCompanyRoles;

            if (!string.IsNullOrEmpty(searchString))
            {
                list = list.Where(tt => tt.Name.ToLower().Contains(searchString.ToLower())).ToList();
            }
            return list;
        }
        #endregion

        #region Actions
        [Permissions(AccessComponent.Roles, AccessLevel.Read)]
        [HttpGet]
        public ActionResult Roles()
        {
            var searchString = (string)TempData["Filters"];
            if (StoredCompanyRoles == null)
            {
                var result = GetAllCompanyRolesResult();
                if (StoredCompanyRoles == null) return result;
            }
            var model = GetCompanyRoles(searchString);
            if (model == null) return SessionHelper.ClearSession();
            ViewBag.SearchString = searchString;
            return View(model);
        }

        private ActionResult GetAllCompanyRolesResult()
        {
            var response = ServiceProxySingleton.Instance.GetRoleList();
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsAccessDenied()) return RedirectToAction("AccessError", "Error");
            StoredCompanyRoles = response.AttachedObject;
            return null;
        }

        [HttpPost]
        public ActionResult Roles(string searchString)
        {
            TempData["Filters"] = searchString;
            return RedirectToAction("Roles");
        }

        [HttpGet]
        public ActionResult New()
        {
            var response = ServiceProxySingleton.Instance.GetNewRole();
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            return View("RolePartial", response.AttachedObject);
        }

        [HttpPost]
        public ActionResult Edit(int id)
        {
            var response = ServiceProxySingleton.Instance.GetRole(id);
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            if (response.AttachedObject.Type == SystemRoles.Administrator)
            {
                response.AttachedObject.Components.ForEach(
                    c =>
                    {
                        if (Constants.DisabledComponentsForAdmin.Contains(c.ComponentId))
                            c.Disabled = true;
                    });
            }
            return View("RolePartial", response.AttachedObject);
        }

        [HttpPost]
        public ActionResult Copy(int id)
        {
            var response = ServiceProxySingleton.Instance.GetRole(id);
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            response.AttachedObject.RoleId = 0;
            response.AttachedObject.Name = null;
            response.AttachedObject.Type = (int)SystemRoles.None;
            return View("RolePartial", response.AttachedObject);
        }

        [HttpPost]
        public ActionResult Update(RoleModel role)
        {
            var response = ServiceProxySingleton.Instance.SaveRole(role);
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            StoredCompanyRoles = null;
            if (response.CurrentUserPermissions != null)
            {
                SessionHelper.Permissions = response.CurrentUserPermissions;
                return Json(new { response.IsPermissionsChanged });
            }
            role.RoleId = response.EntityId;
            return View("RolePartialRow", role);
        }

        [HttpPost]
        public ActionResult Delete(int roleId)
        {
            var response = ServiceProxySingleton.Instance.DeleteRole(new DeleteArg { Id = roleId });
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            StoredCompanyRoles = null;
            if (response.CurrentUserPermissions != null)
                SessionHelper.Permissions = response.CurrentUserPermissions;
            return new EmptyResult();
        }
        #endregion

    }
}
