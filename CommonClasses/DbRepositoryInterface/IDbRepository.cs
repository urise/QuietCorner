using System;
using System.Collections.Generic;
using CommonClasses.DbClasses;
using CommonClasses.InfoClasses;
using CommonClasses.MethodArguments;
using CommonClasses.MethodResults;
using CommonClasses.Models;
using CommonClasses.Roles;
using Interfaces.Enums;

namespace CommonClasses.DbRepositoryInterface
{
    public interface IDbRepository: IDisposable
    {
        int? InstanceId { get; }
//        int? UserId { get; }
        void SetInstanceId(int instanceId);
        void SetAuthInfo(AuthInfo authInfo);

        int Save<T>(T obj, int? transactionNumber = null) where T : class, IMapping;
        void Delete<T>(int id, string reason = null, int? transactionNumber = null) where T : class, IMapping;
        void Delete<T>(T record, string reason = null, int? transactionNumber = null) where T : class, IMapping;
        T GetById<T>(int id) where T : class, IMapping;

        User GetUserByKey(string key);
        TemporaryCode GetTemporaryCodeByUserId(int userId);
        User GetAndAuthenticateUser(LogonArg arg);
        User GetUserByLogin(string login);
        User GetUserByEmail(string email);
        string GetUserNameByCode(string code);
        int GetLastUsedInstanceId(int userId);
        bool CheckIfUserLinkedToInstance(int userId, int instanceId);
        bool CheckIfUserLinkedToInstance(int userId);
        UserAccess GetUserAccess(int userId);
        Instance GetInstanceById(int id);
        IList<Instance> GetUserInstances();

        bool LoginIsNotUnique(string login);
        bool EmailIsNotUnique(string email, int userId = 0);
        bool IsExistInstanceName(string instanceName);
        void AddUserInstance(int? userId = null);
        void AddUserRole(int roleId, int? userId = null);

        List<string> GetUserInstanceList();
        UserInstance GetUserInstance(string userName);
        int GetSystemRoleId(SystemRoles roleType);
        void DeleteUserRoles(int userId, string reason);
        List<RoleInfo> GetUserRoles(int userId);
        IDictionary<int, int> GetUserRoleIds(int userId);
        string GetUserNamesLinkedToRole(int roleId);
        List<ComponentInfo> GetComponents(int roleId);
        bool IsUniqueRoleName(int roleId, string roleName);
        List<ComponentRole> GetComponentRoles(List<ComponentInfo> components, int roleId);
        bool ExistsUsersLinkedToRole(int roleId);
        List<ComponentRole> GetComponentRoles(int roleId);
        List<RoleModel> GetRoles();
    }
}
