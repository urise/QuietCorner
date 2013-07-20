using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CommonClasses;
using CommonClasses.InfoClasses;
using CommonClasses.MethodArguments;
using CommonClasses.Models;
using ServiceProxy;
using WebSite.Helpers;

namespace WebSite.Controllers
{
    [SessionAuthorize]
    public class UsersController : Controller
    {
        #region  Properties and additional methods

        private List<string> StoredInstanceUsers
        {
            get { return Session[Constants.SESSION_INSTANCE_USERS] as List<string>; }
            set { Session[Constants.SESSION_INSTANCE_USERS] = value; }
        }

        private List<string> GetInstanceUsers(string searchString)
        {
            var list = StoredInstanceUsers;

            if (!string.IsNullOrEmpty(searchString))
            {
                list = list.Where(tt => tt.ToLower().Contains(searchString.ToLower())).ToList();
            }
            return list;
        }
        #endregion

        #region UserInstance
        [HttpGet]
        [Permissions(AccessComponent.Users, AccessLevel.Read)]
        public ActionResult Users()
        {
            var searchString = (string)TempData["Filters"];
            if (StoredInstanceUsers == null)
            {
                var result = GetCompanyUsersResult();
                if (StoredInstanceUsers == null) return result;
            }
            var model = GetInstanceUsers(searchString);
            ViewBag.SearchString = searchString;
            return View(model);
        }

        private ActionResult GetCompanyUsersResult()
        {
            var response = ServiceProxySingleton.Instance.GetUserInstanceList();
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsAccessDenied()) return RedirectToAction("AccessError", "Error");
            StoredInstanceUsers = response.AttachedObject;
            return null;
        }


        [HttpPost]
        [Permissions(AccessComponent.Users, AccessLevel.Read)]
        public ActionResult Users(string searchString)
        {
            TempData["Filters"] = searchString;
            return RedirectToAction("Users");
        }

        [HttpPost]
        public ActionResult AddUser(string userName)
        {
            var response = ServiceProxySingleton.Instance.SaveUserInstance(userName);
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            SessionHelper.ClearUserRolesFromSession();
            return Json(new { UserName = userName });
        }

        [HttpPost]
        public ActionResult RemoveUser(string userName)
        {
            var response = ServiceProxySingleton.Instance.DeleteUserInstance(userName);
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            SessionHelper.ClearUserRolesFromSession();
            return new EmptyResult();
        }
        #endregion

        #region Edit User Role
//        [HttpGet]
//        [Permissions(AccessComponent.Users, AccessLevel.Read)]
//        public ActionResult UserRoles()
//        {
//            return View(new UserRoles());
//        }

        [HttpPost]
        public ActionResult UserInfo(string userName)
        {
            var response = ServiceProxySingleton.Instance.GetUserInfo(userName);
            if (response.IsNotLoggedIn()) return SessionHelper.ClearSession();
            if (response.IsError()) return Json(response);
            if (response.IsAccessDenied()) RedirectToAction("AccessError", "Error");
            return View(response.AttachedObject);
        }

        [HttpPost]
        public ActionResult SaveUser(UserInfo userInfo)
        {
            var result = ServiceProxySingleton.Instance.SaveUserInfo(userInfo);
            if (result.IsError()) return Json(result);
            if (result.CurrentUserPermissions != null)
                SessionHelper.Permissions = result.CurrentUserPermissions;
            SessionHelper.ClearUserRolesFromSession();
            return new EmptyResult();
        }

        #endregion
    }
}
