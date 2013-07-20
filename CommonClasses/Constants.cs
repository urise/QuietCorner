using System.Collections.Generic;

namespace CommonClasses
{
    public enum AuthenticationType
    {
        Native = 1,
        ActiveDirectory = 2
    }

    public enum AccessComponent
    {
        None = 0,
        Home = 1,
        Instance = 2,
        Settings = 3,
        Users = 4,
        Roles = 5
    }

    public enum AccessLevel
    {
        None = 1, 
        Read = 2, 
        ReadWrite = 3
    }

    public static class Constants
    {
        public const string DevToken = "_devtoken_";

        #region Session keys
        public const string SESSION_AUTH_INFO = "AuthToken";
        public const string SESSION_PERMISSIONS = "Permissions";
        public const string SESSION_INSTANCE_ID = "InstanceId";
        public const string SESSION_VIEW_INSTANCE_NAME = "InstanceName";
        public const string SESSION_USER_NAME = "UserName";
        public const string SESSION_FORCED_LOGOUT = "ForcedOut";
        public const string SESSION_LAST_LOGGED_INSTANCE = "LastLoggedInstance";
        public const string SESSION_INSTANCE_USERS = "InstanceUsers";
        public const string SESSION_INSTANCE_ROLES = "InstanceRoles";
        #endregion 

        #region Predefined
        public static readonly List<int> ComponentsForGuest = new List<int>
        {
            (int)AccessComponent.Home
        };

        public static readonly List<int> DisabledComponentsForAdmin = new List<int>
        {
            (int)AccessComponent.Roles,
            (int)AccessComponent.Users
        };

        #endregion
    }


    public static class RegexExpressions
    {
        public const string EmailRegex = @"[A-Za-z\d]+([-+.'][A-Za-z\d]+)*@[A-Za-z\d]+([-.][A-Za-z\d]+)*\.[A-Za-z\d]+([-.][A-Za-z\d]+)*";
    }
}
