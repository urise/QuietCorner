using System;
using System.Collections.Generic;
using BusinessLayer.Authentication;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.DbRepositoryInterface;
using CommonClasses.Helpers;
using CommonClasses.InfoClasses;
using CommonClasses.MethodArguments;
using CommonClasses.MethodResults;
using Interfaces.Enums;
using CommonClasses.Models;

namespace BusinessLayer.Managers
{
    public class LoginManager: CommonManager
    {
        #region Constructors

        public LoginManager() { }

        public LoginManager(IDbRepository repository) : base(repository) { }

        #endregion

        #region Login
        private User GetUserByLoginAndPassword(LogonArg arg)
        {
            switch (AppConfiguration.AuthenticationMethod)
            {
                case AuthenticationType.Native:
                    return Db.GetAndAuthenticateUser(arg);
                case AuthenticationType.ActiveDirectory:
                    var adHelper = new AdHelper(AppConfiguration.LDAPServer);
                    BaseResult adResult = adHelper.Authenticate(arg.Login, arg.PasswordHash);
                    if (adResult.IsError()) return null;
                    return Db.GetUserByLogin(arg.Login);
            }
            return null;
        }

        public LoginResult Logon(LogonArg arg)
        {
            var user = GetUserByLoginAndPassword(arg);
            if (user == null)
                return new LoginResult { ErrorMessage = Messages.WrongLoginOrPassword };
            if (!user.IsActive)
                return new LoginResult { ErrorMessage = Messages.UserEmailUnapproved };

            var authInfo = new AuthInfo { UserId = user.UserId };

            var token = AuthTokens.Instance.AddAuth(authInfo);

            return arg.DefaultInstanceId != 0
                        ? LogonToInstance(token, arg.DefaultInstanceId)
                        : LogonUserWithoutInstance(token, user.UserId);
        }


        private LoginResult LogonUserWithoutInstance(string token, int userId)
        {
            return new LoginResult { Token = token, LastUsedInstanceId = Db.GetLastUsedInstanceId(userId) };
        }

        public LoginResult LogonToInstance(string token, int instanceId)
        {
            var authInfo = AuthTokens.Instance.GetAuth(token);
            if (authInfo == null)
            {
                return new LoginResult { ResultType = ResultTypeEnum.NotLoggedIn };
            }

            if (!Db.CheckIfUserLinkedToInstance(authInfo.UserId, instanceId))
            {
                authInfo.InstanceId = 0;
                return new LoginResult { ErrorMessage = Messages.UserInstanceDoesntMatch };
            }

            var instance = Db.GetInstanceById(instanceId);
            if (instance == null)
            {
                return new LoginResult { ErrorMessage = Messages.InstanceNotFound };
            }

            authInfo.InstanceId = instanceId;
            Db.SetAuthInfo(authInfo);
            authInfo.UserAccess = Db.GetUserAccess(authInfo.UserId);

            LogUsageToDb(authInfo.UserId, instanceId);

            return new LoginResult
            {
                Token = token,
                InstanceId = instanceId,
                InstanceName = instance.InstanceName,
                Access = authInfo.UserAccess
            };
        }
       
        private void LogUsageToDb(int userId, int companyId)
        {
            var usageLog = new InstanceUsage { LoginDate = DateTime.Now, InstanceId = companyId, UserId = userId };
            Db.Save(usageLog);
        }

        public MethodResult<IList<Instance>> GetUserInstances()
        {
            return new MethodResult<IList<Instance>>(Db.GetUserInstances());
        }
         
        #endregion

        #region Logout
        public BaseResult Logout(string token)
        {
            if (token != null)
            {
                if (AuthTokens.Instance.RemoveAuth(token))
                    return new BaseResult();
            }
            return new BaseResult { ResultType = ResultTypeEnum.Error };
        }
        #endregion

        
        #region RegisterUser
        public MethodResult<string> RegisterUser(RegisterUser registerUser)
        {
            if (Db.LoginIsNotUnique(registerUser.Login))
                return new MethodResult<string> { ErrorMessage = Messages.LoginAlreadyUsed };
            if (Db.EmailIsNotUnique(registerUser.Email))
                return new MethodResult<string> { ErrorMessage = Messages.EmailAlreadyUsed };
            registerUser.RegistrationCode = RandomHelper.GetRandomString(10);
            var user = new User();
            ReflectionHelper.CopyAllProperties(registerUser, user);
            Db.Save(user);
            return new MethodResult<string>(user.RegistrationCode);
        }

        public BaseResult ConfirmUserKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return new BaseResult { ResultType = ResultTypeEnum.Error };

            var user = Db.GetUserByKey(key);
            if (user == null)
                return new BaseResult { ResultType = ResultTypeEnum.Error };
            user.IsActive = true;
            user.RegistrationCode = null;

            Db.Save(user);
            return new BaseResult();
        }
        #endregion

        #region Change/Forgot Password
        public BaseResult ChangePassword(UserPassword userPassword)
        {
            var user = Db.GetAndAuthenticateUser(
                    new LogonArg
                    {
                        Login = userPassword.UserName,
                        PasswordHash = userPassword.OldPassword,
                        Salt = userPassword.Salt
                    });
            if (user == null)
                return new BaseResult { ErrorMessage = Messages.UserNotFoundByPassword };
            if (user.Password == userPassword.Password)
                return new BaseResult { ErrorMessage = Messages.NewPasswordIsNotDifferentFromTheOld };

            user.Password = userPassword.Password;
            Db.Save(user);
            return new BaseResult();
        }

        public BaseResult ForgotPassword(UserPassword userPassword)
        {
            var user = Db.GetUserByLogin(userPassword.UserName);
            if (user == null)
                return new LoginResult { ErrorMessage = Messages.WrongLogin };

            var temporaryCode = Db.GetTemporaryCodeByUserId(user.UserId);
            if (temporaryCode == null || temporaryCode.Code != userPassword.Code) 
                return new BaseResult { ErrorMessage = Messages.CantForgotPassword };
            if (temporaryCode.ExpireDate < DateTime.Now)
                return new BaseResult { ErrorMessage = Messages.TemporaryCodeExpired };

            Db.Delete(temporaryCode);

            user.Password = userPassword.Password;
            Db.Save(user);
            return new BaseResult();
        }

        public MethodResult<PasswordMailInfo> CreateTemporaryCode(string nameOrEmail)
        {
            if (string.IsNullOrEmpty(nameOrEmail))
                return new MethodResult<PasswordMailInfo> { ResultType = ResultTypeEnum.Error };

            User user;
            if (nameOrEmail.Contains("@"))
            {
                user = Db.GetUserByEmail(nameOrEmail);
                if (user == null) return new MethodResult<PasswordMailInfo> { ErrorMessage = Messages.UserNotFoundByEmail };
            }
            else
            {
                user = Db.GetUserByLogin(nameOrEmail);
                if (user == null) return new MethodResult<PasswordMailInfo> { ErrorMessage = Messages.UserNotFoundByLogin };
            }

            var oldCode = Db.GetTemporaryCodeByUserId(user.UserId);
            var newCode = new TemporaryCode
            {
                TemporaryCodeId = oldCode != null ? oldCode.TemporaryCodeId : 0,
                UserId = user.UserId,
                Code = RandomHelper.GetRandomString(10),
                ExpireDate = DateTime.Now.AddHours(AppConfiguration.PasswordLinkTtl)
            };
            Db.Save(newCode);

            var info = new PasswordMailInfo
            {
                UserName = user.UserFio ?? user.Login,
                Email = user.Email,
                Code = newCode.Code
            };
            return new MethodResult<PasswordMailInfo>(info);
        }

        public MethodResult<UserPassword> GetUserPasswordByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return new MethodResult<UserPassword> { ErrorMessage = Messages.CantForgotPassword };

            var userName = Db.GetUserNameByCode(code);
            return string.IsNullOrEmpty(userName) ?
                new MethodResult<UserPassword> { ErrorMessage = Messages.CantForgotPassword } :
                new MethodResult<UserPassword>(new UserPassword(userName, false, code));
        }

        #endregion

        public BaseResult Test()
        {
            Db.Save(new Instance {InstanceId = 1, InstanceName = "bbb"});
            return null;
        }
        
    }
}
