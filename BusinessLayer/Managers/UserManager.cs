using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.InfoClasses;
using CommonClasses.MethodResults;
using CommonClasses.Models;
using Interfaces.Enums;

namespace BusinessLayer.Managers
{
    public class UserManager: CommonManager
    {
        #region Manage UserInstance
        public MethodResult<List<string>> GetUserInstanceList()
        {
            return new MethodResult<List<string>>(Db.GetUserInstanceList());
        }

        public BaseResult SaveUserInstance(string loginOrEmail)
        {
            var isEmailEntered = loginOrEmail.Contains("@");
            var user = isEmailEntered ? Db.GetUserByEmail(loginOrEmail) : Db.GetUserByLogin(loginOrEmail);
            if (user == null)
                return new MethodResult<Tuple<int, string>> { ErrorMessage = isEmailEntered ? Messages.UserNotFoundByEmail : Messages.UserNotFoundByLogin };
            if (Db.CheckIfUserLinkedToInstance(user.UserId))
                return new MethodResult<Tuple<int, string>> { ErrorMessage = Messages.UserInstanceAlreadyExist };

            var roleId = Db.GetSystemRoleId(SystemRoles.Guest);
            Db.AddUserInstance(user.UserId);
            Db.AddUserRole(roleId, user.UserId);
            return new BaseResult();
        }

        public BaseResult DeleteUserInstance(string userName)
        {
            var userCompany = Db.GetUserInstance(userName);
            if (userCompany == null)
                return new BaseResult { ErrorMessage = Messages.UserInstanceNotFound };
            Db.DeleteUserRoles(userCompany.UserId, string.Empty);
            Db.Delete(userCompany);
            return new BaseResult();
        }

        #endregion

        #region Edit User/UserRoles

        public MethodResult<UserInfo> GetUserInfo(string userName)
        {
            var user = Db.GetUserByLogin(userName);
            if (user == null)
                return new MethodResult<UserInfo> { ErrorMessage = Messages.UserNotFoundByLogin };

            return new MethodResult<UserInfo>(new UserInfo
                {
                    Login = userName,
                    Email = user.Email,
                    UserFio = user.UserFio,
                    UserRoles = Db.GetUserRoles(user.UserId)
                });
        }

        public ChangePermissionsResult SaveUserInfo(UserInfo userInfo)
        {
            var user = Db.GetUserByLogin(userInfo.Login);
            if (user == null)
                return new ChangePermissionsResult { ErrorMessage = Messages.UserNotFoundByLogin };
            if (userInfo.UserRoles == null || !userInfo.UserRoles.Any())
                return new ChangePermissionsResult { ErrorMessage = Messages.UserRolesNotFound };

            var emailChanged = user.Email != userInfo.Email;
            var userInfoChanged = user.UserFio != userInfo.UserFio || emailChanged;
            if (emailChanged && Db.EmailIsNotUnique(userInfo.Email, user.UserId))
                return new ChangePermissionsResult { ErrorMessage = Messages.EmailAlreadyUsed };

            using (var transaction = new TransactionScope())
            {
                if (userInfoChanged)
                {
                    user.UserFio = userInfo.UserFio;
                    user.Email = userInfo.Email;
                    Db.Save(user);
                }

                bool isPermissionChanged = false;
                var userRoleIds = Db.GetUserRoleIds(user.UserId);
                foreach (var userRole in userInfo.UserRoles)
                {
                    var userRoleId = userRoleIds != null && userRoleIds.ContainsKey(userRole.RoleId) ? userRoleIds[userRole.RoleId] : 0;
                    if (userRoleId == 0 && userRole.IsUsed)
                    {
                        Db.AddUserRole(userRole.RoleId, user.UserId);
                        if (!isPermissionChanged) isPermissionChanged = true;
                    }
                    else if (userRoleId != 0 && !userRole.IsUsed)
                    {
                        Db.Delete<UserRole>(userRoleId, string.Empty);
                        if (!isPermissionChanged) isPermissionChanged = true;
                    }

                }

                transaction.Complete();
                return new ChangePermissionsResult
                {
                    IsPermissionsChanged = isPermissionChanged,
                    EntityId = user.UserId
                };
            
            }

        }
        #endregion
    }
}
