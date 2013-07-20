using System;
using System.Globalization;
using System.Linq;
using CommonClasses.DbClasses;
using CommonClasses.Helpers;
using CommonClasses.Roles;
using Interfaces.MiscInterfaces;

namespace CommonClasses.InfoClasses
{
    public class AuthInfo
    {
        public int UserId { get; set; }
        public int InstanceId { get; set; }
        public DateTime LastActiveDate { get; set; }
        public UserAccess UserAccess { get; set; }

        public int AuthTokenId { get; set; }
        public string Token { get; set; }
        public int LinkedInstanceId
        {
            get { return InstanceId; }
            set { InstanceId = value; }
        }

        public AuthInfo()
        {
            UserAccess = new UserAccess();
        }

        public AuthInfo(int userId, int companyId)
        {
            UserId = userId;
            InstanceId = companyId;
        }

        public bool AccessGranted(System.Reflection.MethodBase method)
        {
            var attributes = Attribute.GetCustomAttributes(method).Where(r => r is AccessTier).ToArray();
            return attributes.Length == 0 || attributes.Cast<AccessTier>().Any(accessTier => UserAccess.IsGranted(accessTier.Component, accessTier.Level));
        }
    }
}