using System;
using System.Collections;
using System.Transactions;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.Helpers;
using CommonClasses.MethodResults;
using Interfaces.Enums;

namespace BusinessLayer.Managers
{
    public class InstanceManager: CommonManager
    {
        #region Create Save Company

        public MethodResult<int> CreateInstance(string instanceName)
        {
            using (var transaction = new TransactionScope())
            {
                var instance = new Instance
                {
                    InstanceName = instanceName
                };
                var validateError = ValidateInstance(instance);
                if (!string.IsNullOrEmpty(validateError))
                    return new MethodResult<int> { ErrorMessage = validateError };

                Db.Save(instance);
                Db.SetInstanceId(instance.InstanceId);

                var adminRoleId = InsertSystemRoles(instance.InstanceId);
                Db.AddUserInstance();
                Db.AddUserRole(adminRoleId);
                transaction.Complete();
                return new MethodResult<int>(instance.InstanceId);

            }
        }
        
        public string ValidateInstance(Instance instance)
        {
            if (string.IsNullOrEmpty(instance.InstanceName))
                return Messages.EmptyCompanyName;
            if (Db.IsExistInstanceName(instance.InstanceName))
                return Messages.ExistsCompanyName;
            return null;
        }

        private int InsertSystemRoles(int instanceId)
        {
            var adminRoleId = InsertRoleAndAccess(instanceId, SystemRoles.Administrator, Enum.GetValues(typeof(AccessComponent)), AccessLevel.ReadWrite);
            InsertRoleAndAccess(instanceId, SystemRoles.Guest, Constants.ComponentsForGuest, AccessLevel.Read);
            return adminRoleId;
        }

        private int InsertRoleAndAccess(int instanceId, SystemRoles systemRole, IEnumerable components, AccessLevel accessLevel)
        {
            var role = new Role
            {
                InstanceId = instanceId,
                RoleType = systemRole,
                RoleName = systemRole.GetDescription()
            };
            Db.Save(role);
            foreach (var component in components)
            {
                if ((int)component == (int)AccessComponent.None)
                    continue;
                var componentsToRole = new ComponentRole
                {
                    InstanceId = instanceId,
                    AccessLevel = accessLevel,
                    ComponentId = (int)component,
                    RoleId = role.RoleId
                };
                Db.Save(componentsToRole);
            }
            return role.RoleId;
        }

        #endregion
    }
}
