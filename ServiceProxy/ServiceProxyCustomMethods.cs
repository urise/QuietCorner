using System;
using System.Collections.Generic;
using CommonClasses.InfoClasses;
using CommonClasses.MethodArguments;
using CommonClasses.MethodResults;
using CommonClasses.Models;
using CommonClasses.DbClasses;

namespace ServiceProxy
{
    public partial class ServiceProxySingleton
    {
        #region ChangePassword

        public BaseResult ChangePassword(UserPassword userPassword)
        {
            Delay();
            return SendPostRequest<BaseResult, UserPassword>("changePassword", userPassword);
        }

        public BaseResult ForgotPassword(UserPassword userPassword)
        {
            Delay();
            return SendPostRequest<BaseResult, UserPassword>("forgotPassword", userPassword, false);
        }

        public MethodResult<PasswordMailInfo> CreateTemporaryCode(string nameOrEmail)
        {
            Delay();
            return SendPostRequest<MethodResult<PasswordMailInfo>, string>("createTemporaryCode", nameOrEmail, false);
        }

        public MethodResult<UserPassword> GetUserPasswordByCode(string code)
        {
            Delay();
            return SendGetRequest<MethodResult<UserPassword>>("getUserPasswordByCode", code, false);
        }
        #endregion

        #region Instance
        public MethodResult<int> CreateInstance(string instanceName)
        {
            Delay();
            return SendPostRequest<MethodResult<int>, string>("createInstance", instanceName);
        }
        #endregion

        #region Login

        public LoginResult Logon(LogonArg arg)
        {
            return SendPostRequest<LoginResult, LogonArg>("logon", arg, false);
        }

        public LoginResult LogonToInstance(int instanceId)
        {
            Delay();
            return SendPostRequest<LoginResult, int>("logonToInstance", instanceId);
        }

        public BaseResult Logout()
        {
            Delay();
            return SendGetRequest<BaseResult>("logout");
        }

        public MethodResult<IList<Instance>> GetUserInstances()
        {
            Delay();
            return SendGetRequest<MethodResult<IList<Instance>>>("getUserInstances");
        }

        #endregion

        #region Register

        public MethodResult<string> RegisterUser(RegisterUser user)
        {
            Delay();
            return SendPostRequest<MethodResult<string>, RegisterUser>("registerUser", user, false);
        }

        public BaseResult ConfirmUserKey(string key)
        {
            Delay();
            return SendGetRequest<BaseResult>("confirmUserKey", key, false);
        }

        #endregion

        #region Roles

        public MethodResult<List<RoleModel>> GetRoleList()
        {
            return SendGetRequest<MethodResult<List<RoleModel>>>("getRoleList");
        }

        public ChangePermissionsResult SaveRole(RoleModel role)
        {
            return SendPostRequest<ChangePermissionsResult, RoleModel>("saveRole", role);
        }

        public ChangePermissionsResult DeleteRole(DeleteArg arg)
        {
            return SendPostRequest<ChangePermissionsResult, DeleteArg>("deleteRole", arg);
        }

        public MethodResult<RoleModel> GetNewRole()
        {
            return SendGetRequest<MethodResult<RoleModel>>("getNewRole");
        }

        public MethodResult<RoleModel> GetRole(int id)
        {
            return SendGetRequest<MethodResult<RoleModel>>("getRole", "?id=" + id);
        }

        #endregion

        #region Users Methods
        public MethodResult<List<string>> GetUserInstanceList()
        {
            Delay();
            return SendGetRequest<MethodResult<List<string>>>("getUserInstanceList");
        }

        public BaseResult SaveUserInstance(string userName)
        {
            Delay();
            return SendPostRequest<BaseResult, string>("saveUserInstance", userName);
        }

        public BaseResult DeleteUserInstance(string userName)
        {
            Delay();
            return SendPostRequest<BaseResult, string>("deleteUserInstance", userName);
        }

        public MethodResult<UserInfo> GetUserInfo(string userName)
        {
            Delay();
            return SendPostRequest<MethodResult<UserInfo>, string>("getUserInfo", userName);
        }

        public ChangePermissionsResult SaveUserInfo(UserInfo userInfo)
        {
            Delay();
            return SendPostRequest<ChangePermissionsResult, UserInfo>("saveUserInfo", userInfo);
        }
        #endregion
    }
}
