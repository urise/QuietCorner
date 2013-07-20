using System;
using System.Collections.Generic;
using System.Linq;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.DbRepositoryInterface;
using CommonClasses.Helpers;
using CommonClasses.InfoClasses;
using CommonClasses.MethodArguments;
using CommonClasses.Models;
using CommonClasses.Roles;
using Interfaces.Enums;

namespace DbLayer.Repositories
{
    public class DbRepository: IDbRepository
    {
        #region Properties and Variables

        private FilteredContext _context;
        private FilteredContext Context
        {
            get { return _context; }
        }

        private int? _instanceId;
        public int? InstanceId
        {
            get { return _instanceId; }
        }
        private bool _releaseContext;
        private AuthInfo _authInfo;

        public int? UserId
        {
            get { return _authInfo == null ? (int?)null : _authInfo.UserId; }
        }

        public void SetInstanceId(int instanceId)
        {
            if (_instanceId != 0)
                throw new Exception("Cannot set instance id for DbRepository if it's already defined");
            _authInfo.InstanceId = instanceId;
            _instanceId = instanceId;
            _context.SetInstanceId(instanceId);
        }

        public void SetAuthInfo(AuthInfo authInfo)
        {
            if (_authInfo != null)
                throw new Exception("Cannot set auth info for DbRepository if it's already defined");
            _authInfo = authInfo;
            _instanceId = authInfo.InstanceId;
            _context.SetInstanceId(authInfo.InstanceId);
        }
        #endregion

        #region Constructors

        public DbRepository(int? instanceId)
        {
            _context = new FilteredContext(instanceId);
            _instanceId = instanceId;
            _releaseContext = true;
        }

        public DbRepository(AuthInfo authInfo)
        {
            _authInfo = authInfo;
            _instanceId = authInfo.InstanceId;
            _context = new FilteredContext(authInfo.InstanceId);
            _releaseContext = true;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_releaseContext)
                _context.Dispose();
        }

        #endregion

        #region Logging

        private void LogToDb(int? userId, string tableName, int recordId, string operation, string details, int? transactionNumber)
        {
            InsertLogToDb(_instanceId, userId, tableName, recordId, operation, details, transactionNumber);
        }

        private void InsertLogToDb(int? instanceId, int? userId, string tableName, int recordId, string operation, string details, int? transactionNumber)
        {
            var dataLog = new DataLog
            {
                InstanceId = instanceId == 0 ? null: instanceId,
                UserId = userId == 0 ? null : userId,
                OperationTime = DateTime.Now,
                TableName = tableName,
                RecordId = recordId,
                Operation = operation,
                Details = details,
                TransactionNumber = transactionNumber
            };
            _context.Add(dataLog);
            _context.SaveChanges();
        }

        #endregion

        #region Save

        public int Save<T>(T obj, int? transactionNumber = null) where T : class, IMapping
        {
            if (obj is IConstraintedByInstanceId && (obj as IConstraintedByInstanceId).InstanceId != InstanceId)
            {
                throw new Exception("Save: wrong InstanceId for object type " + obj.GetType().Name + ".");
            }

            int id = obj.PrimaryKeyValue;
            string diffXml = string.Empty;
            if (id == 0)
            {
                _context.Add(obj);
            }
            else
            {
                var record = _context.GetById<T>(id);
                diffXml = XmlHelper.GetDifferenceXml(record, obj);
                ReflectionHelper.CopyAllProperties(obj, record);
            }
            _context.SaveChanges();
            
            if (id == 0)
            {
                LogToDb(UserId, typeof(T).Name, obj.PrimaryKeyValue, "I", XmlHelper.GetObjectXml(obj), transactionNumber);
            }
            else
            {
                LogToDb(UserId, typeof(T).Name, obj.PrimaryKeyValue, "U", diffXml, transactionNumber);
            }
            return obj.PrimaryKeyValue;
        }

        #endregion

        #region Delete

        public void Delete<T>(int id, string reason = null, int? transactionNumber = null) where T : class, IMapping
        {
            var record = _context.GetById<T>(id);
            Delete(record, reason, transactionNumber);
        }

        public void Delete<T>(T record, string reason = null, int? transactionNumber = null) where T : class, IMapping
        {
            LogToDb(UserId, typeof(T).Name, record.PrimaryKeyValue, "D", XmlHelper.GetObjectXml(record, reason), transactionNumber);
            _context.Remove(record);
            _context.SaveChanges();
        }

        #endregion

        #region Get

        public T GetById<T>(int id) where T : class, IMapping
        {
            return _context.GetById<T>(id);
        }

        #endregion

        #region Methods

        #endregion

        #region Login

        public User GetUserByLogin(string login)
        {
            return Context.Users.FirstOrDefault(x => x.Login.ToLower() == login.ToLower());
        }

        public User GetAndAuthenticateUser(LogonArg arg)
        {
            var user = GetUserByLogin(arg.Login);
            if (user != null)
            {
                var hash = CryptHelper.GetSha512Base64Hash(arg.Salt + user.Password);
                if (hash.Equals(arg.PasswordHash))
                    return user;
            }
            return null;
        }

        public int GetLastUsedInstanceId(int userId)
        {
            var lastRecord = Context.InstanceUsages.Where(ucu => ucu.UserId == userId).OrderBy(ucu => ucu.LoginDate).AsEnumerable().LastOrDefault();
            return lastRecord == null ? 0 : lastRecord.InstanceId;
        }

        public bool CheckIfUserLinkedToInstance(int userId, int instanceId)
        {
            return Context.UserInstances.Any(x => x.UserId == userId && x.InstanceId == instanceId);
        }

        public bool CheckIfUserLinkedToInstance(int userId)
        {
            return Context.UserInstances.Any(x => x.UserId == userId && x.InstanceId == InstanceId);
        }

        public bool LoginIsNotUnique(string login)
        {
            return Context.Users.Any(u => u.Login.ToLower() == login.ToLower());
        }

        public bool EmailIsNotUnique(string email, int userId = 0)
        {
            return Context.Users.Any(u => u.Email == email && u.UserId != userId);
        }

        public User GetUserByKey(string key)
        {
            return Context.Users.FirstOrDefault(u => u.RegistrationCode == key);
        }

        public User GetUserByEmail(string email)
        {
            return Context.Users.FirstOrDefault(u => u.Email == email);
        }

        public string GetUserNameByCode(string code)
        {
            return Context.TemporaryCodes.Where(x => x.Code == code).Select(x => x.User.Login).FirstOrDefault();
        }

        public TemporaryCode GetTemporaryCodeByUserId(int userId)
        {
            return Context.TemporaryCodes.FirstOrDefault(x => x.UserId == userId);
        }

        public IList<Instance> GetUserInstances()
        {
            return (from instance in Context.Instances
                       join userInstances in Context.UserInstances.Where(u=>u.UserId == UserId) 
                        on instance.InstanceId equals userInstances.InstanceId
                            select instance).AsEnumerable().OrderBy(i=>i.InstanceName).ToList();
        }

        public Instance GetInstanceById(int id)
        {
            return Context.Instances.FirstOrDefault(i=>i.InstanceId == id);
        }


        public UserAccess GetUserAccess(int userId)
        {
            var q = from ur in Context.UserRoles
                    join cr in Context.ComponentRoles on ur.RoleId equals cr.RoleId
                    where ur.UserId == userId
                    group cr by cr.ComponentId into accesses
                    select new { Component = (AccessComponent)accesses.Key, Level = accesses.Max(l => l.AccessLevel) };

            var result = new UserAccess();
            foreach (var r in q) result.Add(r.Component, r.Level);
            return result;
        }

        #endregion

        #region Instance
        public bool IsExistInstanceName(string instanceName)
        {
            return Context.Instances.Any(i => i.InstanceName == instanceName);
        }

        public void AddUserRole(int roleId, int? userId = null)
        {
            if (!InstanceId.HasValue || !UserId.HasValue)
                throw new Exception(Messages.ErrorCompanyCreation);
            var userRole = new UserRole { InstanceId = InstanceId.Value, RoleId = roleId, UserId = userId == null ? UserId.Value : userId.Value };
            Save(userRole);
        }

        #endregion

        #region Roles
        public int GetSystemRoleId(SystemRoles roleType)
        {
            return Context.Roles.First(r => r.RoleType == roleType).RoleId;
        }

        public List<RoleModel> GetRoles()
        {
            var roles = (from role in Context.Roles
                         select role)
                  .OrderBy(r => r.RoleName).ToList();

            var model = new List<RoleModel>();
            foreach (var r in roles)
            {
                model.Add(new RoleModel(r) { UserNames = GetUserNamesLinkedToRole(r.RoleId) });
            }
            return model;
        }

        public string GetUserNamesLinkedToRole(int roleId)
        {
            var userNames = (from userRoles in Context.UserRoles
                             join users in Context.Users on userRoles.UserId equals users.UserId
                             where userRoles.RoleId == roleId
                             select users.Login).ToArray();
            return string.Join(",", userNames);
        }

        public List<ComponentInfo> GetComponents(int roleId)
        {
            var components =
                from c in Context.Components
                join ctr in Context.ComponentRoles.Where(x => x.RoleId == roleId)
                on c.ComponentId equals ctr.ComponentId
                into a
                from b in a.AsEnumerable().Select(x => x.AccessLevel).DefaultIfEmpty(AccessLevel.None)
                select new {c, b};
            var list = new List<ComponentInfo>();
            foreach (var component in components)
            {
                list.Add(new ComponentInfo(component.c, component.b));
            }

            return list;
        }

        public bool IsUniqueRoleName(int roleId, string roleName)
        {
            return Context.Roles.Any(p => p.RoleId != roleId && p.RoleName == roleName);
        }

        public List<int> GetUserIdsLinkedToRole(int roleId)
        {
            return Context.UserRoles.Where(ur => ur.RoleId == roleId).AsEnumerable().Select(ur => ur.UserId).ToList();
        }

        public List<ComponentRole> GetComponentRoles(List<ComponentInfo> components, int roleId)
        {
            return (from c in components
                                    join ctr in Context.ComponentRoles.Where(x => x.RoleId == roleId)
                                        on c.ComponentId equals ctr.ComponentId
                                        into a
                                    from b in a.DefaultIfEmpty(new ComponentRole
                                        {
                                            InstanceId = InstanceId.Value,
                                            RoleId = roleId,
                                            ComponentId = c.ComponentId
                                        })
                                    select c.Update(b)).ToList();
        }

        public bool ExistsUsersLinkedToRole(int roleId)
        {
            return Context.UserRoles.Any(ur => ur.RoleId == roleId);
        }

        public List<ComponentRole> GetComponentRoles(int roleId)
        {
            return Context.ComponentRoles.Where(ur => ur.RoleId == roleId).ToList();
        }
        #endregion

        #region User Instance
        public List<string> GetUserInstanceList()
        {
            return (from user in Context.Users
                    join uc in Context.UserInstances on user.UserId equals uc.UserId
                    where uc.InstanceId == InstanceId
                    select user.Login)
                    .OrderBy(u=>u).AsEnumerable().ToList();
        }

        public void AddUserInstance(int? userId = null)
        {
            if (!InstanceId.HasValue || !UserId.HasValue)
                throw new Exception(Messages.ErrorCompanyCreation);
            var userInstance = new UserInstance { InstanceId = InstanceId.Value, UserId = userId == null ? UserId.Value : userId.Value };
            Save(userInstance);
        }

        public UserInstance GetUserInstance(string userName)
        {
            return (from user in Context.Users
                    join uc in Context.UserInstances on user.UserId equals uc.UserId
                    where user.Login == userName
                    select uc).FirstOrDefault();
        }


        public void DeleteUserRoles(int userId, string reason)
        {
            foreach (var userToRole in GetUserToRoles(userId))
            {
                Delete(userToRole, reason);
            }
        }

        public List<UserRole> GetUserToRoles(int userId)
        {
            return Context.UserRoles.Where(ur => ur.UserId == userId).AsEnumerable().ToList();
        }

        public bool IsCurrentUserAdministrator()
        {
            return (from role in Context.Roles
                    join userRoles in Context.UserRoles on role.RoleId equals userRoles.RoleId
                    where userRoles.UserId ==  UserId && role.RoleType == SystemRoles.Administrator
                    select userRoles.UserRoleId).Any();

        }

        public List<RoleInfo> GetUserRoles(int userId)
        {
            var cantChangeAdministratorRole = UserId == userId || !IsCurrentUserAdministrator();
            return (from role in Context.Roles
                    join userToRoles in Context.UserRoles.Where(ur => ur.UserId == userId) on role.RoleId equals userToRoles.RoleId
                    into sub
                    from userRoles in sub.DefaultIfEmpty()
                    select new RoleInfo
                    {
                        RoleId = role.RoleId,
                        Name = role.RoleName,
                        Type = role.RoleType,
                        IsUsed = userRoles != null,
                        IsReadOnly = role.RoleType == SystemRoles.Administrator && cantChangeAdministratorRole
                    })
                    .OrderBy(u => u.Name).AsEnumerable().ToList();
        }

        public IDictionary<int, int> GetUserRoleIds(int userId)
        {
            return Context.UserRoles.Where(x => x.UserId == userId).ToDictionary(x => x.RoleId, x => x.UserRoleId);
        }
        #endregion
    }
}
