using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel.Web;
using BusinessLayer.Authentication;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.Helpers;
using CommonClasses.InfoClasses;
using CommonClasses.MethodArguments;
using CommonClasses.MethodResults;
using CommonClasses.Models;
using CommonClasses.Roles;
using DbLayer.Repositories;
using Interfaces.Enums;
using NLog;
using BusinessLayer.Managers;

namespace RestService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "RestService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select RestService.svc or RestService.svc.cs at the Solution Explorer and start debugging.
    public class RestService : IRestService
    {
        #region Auxilliary Methods

        private Logger _logger = LogManager.GetLogger("AasRest");

        public string AuthToken
        {
            get
            {
                if (WebOperationContext.Current == null) return null;
                IncomingWebRequestContext context = WebOperationContext.Current.IncomingRequest;
                return context.Headers["AuthToken"];
            }
        }

        private string GetPreviousMethodName()
        {
            return GetPreviousMethod().Name;
        }

        private System.Reflection.MethodBase GetPreviousMethod()
        {
            var stackFrames = new StackTrace().GetFrames();
            if (stackFrames != null) return stackFrames[2].GetMethod();
            return null;
        }

        private ResultTypeEnum CheckAccess(string token, out AuthInfo authInfo)
        {
            authInfo = AuthTokens.Instance.GetAuth(token);
            if (authInfo == null)
            {
                return ResultTypeEnum.NotLoggedIn;
            }
            var stackFrames = new StackTrace().GetFrames();
            if (stackFrames != null)
            {
                var callingMethod = stackFrames[2].GetMethod();
                if (!authInfo.AccessGranted(callingMethod))
                {
                    return ResultTypeEnum.AccessDenied;
                }
            }
            return ResultTypeEnum.Success;
        }

        private T RunLoginManagerMethod<T>(Func<LoginManager, T> func) where T : BaseResult, new()
        {
            using (var db = new DbRepository((int?)null))
            {
                try
                {
                    var manager = new LoginManager(db);
                    var result = func(manager);
                    if (result.IsError())
                        _logger.Warn(GetPreviousMethodName() + ": " + result.ErrorMessage);
                    else
                        _logger.Info(GetPreviousMethodName());
                    return result;
                }
                catch (Exception ex)
                {
                    var message = ex.SmartMessage();
                    _logger.Fatal(GetPreviousMethodName() + ": " + message);
                    return new T { ErrorMessage = message };
                }
            }
        }

        private T RunManagerMethod<TClass, T>(Func<TClass, T> func)
            where T : BaseResult, new()
            where TClass : IDbManager, new()
        {
            AuthInfo authInfo;
            
            var checkResult = CheckAccess(AuthToken, out authInfo);
            if (checkResult != ResultTypeEnum.Success)
                return new T { ResultType = checkResult };

            using (var db = new DbRepository(authInfo))
            {
                try
                {
                    var instance = new TClass { Db = db };
                    T result = func(instance);
                    if (result.IsError())
                        _logger.Warn(GetPreviousMethodName() + ": " + result.ErrorMessage);
                    else
                        _logger.Info(GetPreviousMethodName());
                    return result;
                }
                catch (Exception ex)
                {
                    var message = ex.SmartMessage();
                    _logger.Fatal(GetPreviousMethodName() + ": " + message);
                    return new T { ErrorMessage = message };
                }
            }
        }

        private MethodResult<T> RunDbMethodWrapped<T>(Func<DbRepository, T> func)
        {
            AuthInfo authInfo;
            var checkResult = CheckAccess(AuthToken, out authInfo);
            if (checkResult != ResultTypeEnum.Success)
                return new MethodResult<T> { ResultType = checkResult };

            using (var db = new DbRepository(authInfo))
            {
                try
                {
                    T result = func(db);
                    _logger.Info(GetPreviousMethodName());
                    return new MethodResult<T>(result);
                }
                catch (Exception ex)
                {
                    var message = ex.SmartMessage();
                    _logger.Fatal(GetPreviousMethodName() + ": " + message);
                    return new MethodResult<T> { ErrorMessage = message };
                }
            }
        }

        private void ClearAuthCompanyId(string token)
        {
            var authInfo = AuthTokens.Instance.GetAuth(token);
            if (authInfo != null)
            {
                authInfo.InstanceId = 0;
            }
        }

        private void ResetCompanyUserPermissions(ChangePermissionsResult result)
        {
            if (result.IsPermissionsChanged)
                ResetUserPermissions(result.EntityId, result);
        }

        private void ResetRolePermissions(ChangePermissionsResult result)
        {
            if (result.IsPermissionsChanged)
            {
                var dependUsers = RunDbMethodWrapped(rep => rep.GetUserIdsLinkedToRole(result.EntityId)).AttachedObject;
                foreach (var userId in dependUsers)
                    ResetUserPermissions(userId, result);
            }
        }

        /// <summary>
        /// Remove token for edited user (he will get appropriate permissions on next login)
        /// Reset permissions for current user (send new permissions to web and replace it in session)
        /// </summary>
        private void ResetUserPermissions(int userId, ChangePermissionsResult result)
        {
            var authInfo = AuthTokens.Instance.GetAuth(AuthToken);
            if (authInfo.UserId != userId)
            {
                AuthTokens.Instance.RemoveAuthInfoForUser(userId, authInfo.InstanceId);
            }
            else
            {
                authInfo.UserAccess = RunDbMethodWrapped(rep => rep.GetUserAccess(authInfo.UserId)).AttachedObject;
                result.CurrentUserPermissions = authInfo.UserAccess;
            }
        }
        #endregion

        #region Login, Passwords & Register

        public LoginResult Logon(LogonArg arg)
        {
            return RunLoginManagerMethod(lm => lm.Logon(arg));
        }

        public void Test()
        {
            RunLoginManagerMethod(lm => lm.Test());
        }

        public LoginResult LogonToInstance(int instanceId)
        {
            return RunLoginManagerMethod(lm => lm.LogonToInstance(AuthToken, instanceId));
        }
        
        public MethodResult<IList<Instance>> GetUserInstances()
        {
            return RunManagerMethod<LoginManager, MethodResult<IList<Instance>>>(lm => lm.GetUserInstances());
        }

        public BaseResult Logout()
        {
            return RunLoginManagerMethod(lm => lm.Logout(AuthToken));
        }

        public MethodResult<string> RegisterUser(RegisterUser user)
        {
            return RunLoginManagerMethod(lm => lm.RegisterUser(user));
        }

        public BaseResult ConfirmUserKey(string key)
        {
            return RunLoginManagerMethod(lm => lm.ConfirmUserKey(key));
        }

        public BaseResult ChangePassword(UserPassword userPassword)
        {
            return RunManagerMethod<LoginManager, BaseResult>(lm => lm.ChangePassword(userPassword));
        }

        public BaseResult ForgotPassword(UserPassword userPassword)
        {
            return RunLoginManagerMethod(lm => lm.ForgotPassword(userPassword));
        }

        public MethodResult<PasswordMailInfo> CreateTemporaryCode(string nameOrEmail)
        {
            return RunLoginManagerMethod(lm => lm.CreateTemporaryCode(nameOrEmail));
        }

        public MethodResult<UserPassword> GetUserPasswordByCode(string code)
        {
            return RunLoginManagerMethod(lm => lm.GetUserPasswordByCode(code));
        }

        #endregion

        #region Instance

        public MethodResult<int> CreateInstance(string instanceName)
        {
            ClearAuthCompanyId(AuthToken);
            return RunManagerMethod<InstanceManager, MethodResult<int>>(cm => cm.CreateInstance(instanceName));
        }
        #endregion

        #region Roles
        [AccessTier(AccessComponent.Roles, AccessLevel.Read)]
        public MethodResult<List<RoleModel>> GetRoleList()
        {
            return RunManagerMethod<RoleManager, MethodResult<List<RoleModel>>>(rm => rm.GetRoles());
        }

        [AccessTier(AccessComponent.Roles, AccessLevel.ReadWrite)]
        public ChangePermissionsResult SaveRole(RoleModel role)
        {
            var result = RunManagerMethod<RoleManager, ChangePermissionsResult>(rm => rm.SaveRole(role));
            if (result.IsSuccess())
                ResetRolePermissions(result);
            return result;
        }

        [AccessTier(AccessComponent.Roles, AccessLevel.ReadWrite)]
        public ChangePermissionsResult DeleteRole(DeleteArg arg)
        {
            var result = RunManagerMethod<RoleManager, BaseResult>(rm => rm.DeleteCompanyRole(arg));
            var sentResult = new ChangePermissionsResult
            {
                ErrorMessage = result.ErrorMessage,
                ResultType = result.ResultType,
                EntityId = arg.Id,
                IsPermissionsChanged = true
            };
            if (sentResult.IsSuccess())
                ResetRolePermissions(sentResult);
            return sentResult;
        }

        [AccessTier(AccessComponent.Roles, AccessLevel.ReadWrite)]
        public MethodResult<RoleModel> GetNewRole()
        {
            return RunManagerMethod<RoleManager, MethodResult<RoleModel>>(rm => rm.GetRole());
        }

        [AccessTier(AccessComponent.Roles, AccessLevel.ReadWrite)]
        public MethodResult<RoleModel> GetRole(int id)
        {
            return RunManagerMethod<RoleManager, MethodResult<RoleModel>>(rm => rm.GetRole(id));
        }

        #endregion

        #region User
        [AccessTier(AccessComponent.Users, AccessLevel.Read)]
        public MethodResult<List<string>> GetUserInstanceList()
        {
            return RunManagerMethod<UserManager, MethodResult<List<string>>>(rep => rep.GetUserInstanceList());
        }

        [AccessTier(AccessComponent.Users, AccessLevel.ReadWrite)]
        public BaseResult SaveUserInstance(string userName)
        {
            return RunManagerMethod<UserManager, BaseResult>(rep => rep.SaveUserInstance(userName));
        }

        [AccessTier(AccessComponent.Users, AccessLevel.ReadWrite)]
        public BaseResult DeleteUserInstance(string userName)
        {
            return RunManagerMethod<UserManager, BaseResult>(rep => rep.DeleteUserInstance(userName));
        }

        [AccessTier(AccessComponent.Users, AccessLevel.Read)]
        public MethodResult<UserInfo> GetUserInfo(string userName)
        {
            return RunManagerMethod<UserManager, MethodResult<UserInfo>>(rep => rep.GetUserInfo(userName));
        }

        [AccessTier(AccessComponent.Users, AccessLevel.ReadWrite)]
        public ChangePermissionsResult SaveUserInfo(UserInfo userInfo)
        {
            var result = RunManagerMethod<UserManager, ChangePermissionsResult>(rep => rep.SaveUserInfo(userInfo));
            if (result.IsSuccess()) ResetCompanyUserPermissions(result);
            return result;
        }
        #endregion
    }
}
