using System.Web;
using System.Web.Mvc;
using CommonClasses;
using CommonClasses.Roles;

//using CommonClasses.Roles;

namespace WebSite.Helpers
{
    public static class SessionHelper
    {
        public static string UserName
        {
            get
            {
                var session = HttpContext.Current.Session;
                var login = session[Constants.SESSION_USER_NAME];
                return (session != null && login != null) ? login.ToString() : null;
            }
            set
            {
                HttpContext.Current.Session[Constants.SESSION_USER_NAME] = value;
            }
        }

        public static string CompanyName
        {
            get
            {
                var session = HttpContext.Current.Session;
                var companyName = session[Constants.SESSION_VIEW_INSTANCE_NAME];
                return (session != null && companyName != null) ? companyName.ToString() : null;
            }
            set
            {
                HttpContext.Current.Session[Constants.SESSION_VIEW_INSTANCE_NAME] = value;
            }
        }

        public static int? LastUsedInstanceId
        {
            get 
            {
                var session = HttpContext.Current.Session;
                return session == null ? null : (int?)session[Constants.SESSION_LAST_LOGGED_INSTANCE];
            }
            set 
            {
                HttpContext.Current.Session[Constants.SESSION_LAST_LOGGED_INSTANCE] = value;
            }
        }

        public static bool IsAthorized()
        {
            var session = HttpContext.Current.Session;
            return session != null && session[Constants.SESSION_AUTH_INFO] != null;
        }

        public static bool IsInstanceSelected()
        {
            var session = HttpContext.Current.Session;
            return session != null && session[Constants.SESSION_INSTANCE_ID] != null && (int)session[Constants.SESSION_INSTANCE_ID] != 0;
        }

        public static ActionResult ClearSession(string errorMessage = null)
        {
            var session = HttpContext.Current.Session;
            if (session != null)
            {
                session.Clear();
                session[Constants.SESSION_FORCED_LOGOUT] = errorMessage ?? Messages.SessionTimeOut;
            }
            return null;
        }

        public static void ClearCompanyInfoFromSession()
        {
            var session = HttpContext.Current.Session;
            if (session != null)
            {
                session[Constants.SESSION_INSTANCE_ID] = null;
                session[Constants.SESSION_VIEW_INSTANCE_NAME] = null;
                session[Constants.SESSION_INSTANCE_USERS] = null;
                session[Constants.SESSION_INSTANCE_ROLES] = null;
            }
        }

        public static void ClearUserRolesFromSession()
        {
            var session = HttpContext.Current.Session;
            if (session != null)
            {
                session[Constants.SESSION_INSTANCE_USERS] = null;
                session[Constants.SESSION_INSTANCE_ROLES] = null;
            }
        }

        public static UserAccess Permissions
        {
            get
            {
                var session = HttpContext.Current.Session;
                return session == null ? null : (UserAccess)session[Constants.SESSION_PERMISSIONS];
            }
            set
            {
                var session = HttpContext.Current.Session;
                session[Constants.SESSION_PERMISSIONS] = value;
            }
        }
    }
}