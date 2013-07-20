using System.Collections.Generic;
using System.Transactions;
using CommonClasses;
using CommonClasses.DbClasses;
using CommonClasses.MethodArguments;
using CommonClasses.MethodResults;
using CommonClasses.Models;
using Interfaces.Enums;

namespace BusinessLayer.Managers
{
    public class RoleManager : CommonManager
    {
        #region Methods

        public MethodResult<List<RoleModel>> GetRoles()
        {
            var list = Db.GetRoles();
            return new MethodResult<List<RoleModel>>(list);
        }

        public MethodResult<RoleModel> GetRole(int roleId = 0)
        {
            var role = new RoleModel();
            if (roleId != 0)
            {
                var roleDb = Db.GetById<Role>(roleId);
                role.Name = roleDb.RoleName;
                role.Type = roleDb.RoleType;
                role.RoleId = roleDb.RoleId;
                role.UserNames = Db.GetUserNamesLinkedToRole(roleId);
            }
            else
            {
                roleId = Db.GetSystemRoleId(SystemRoles.Guest);
            }
            role.Components = Db.GetComponents(roleId);
            return new MethodResult<RoleModel>(role);

        }

        public ChangePermissionsResult SaveRole(RoleModel role)
        {
            if (role == null)
                return new ChangePermissionsResult { ErrorMessage = Messages.RoleNotFound };
            if (Db.IsUniqueRoleName(role.RoleId, role.Name))
                return new ChangePermissionsResult { ErrorMessage = Messages.RoleNameNotUnique };

            using (var transaction = new TransactionScope())
            {
                var roleDb = new Role
                    {
                        InstanceId = Db.InstanceId.Value,
                        RoleId = role.RoleId,
                        RoleName = role.Name,
                        RoleType = role.Type
                    };
                Db.Save(roleDb);
                var componentsToRole = Db.GetComponentRoles(role.Components, roleDb.RoleId);
                var isPermissionChanged = false;
                foreach (var ctr in componentsToRole)
                {
                    if (ctr.ComponentRoleId != 0 && ctr.AccessLevel == AccessLevel.None)
                    {
                        Db.Delete(ctr);
                        if (!isPermissionChanged) isPermissionChanged = true;
                    }
                    else if ((int)ctr.AccessLevel > (int)AccessLevel.None)
                    {
                        Db.Save(ctr);
                        if (!isPermissionChanged) isPermissionChanged = true;
                    }
                }
                transaction.Complete();
                return new ChangePermissionsResult { EntityId = roleDb.RoleId, IsPermissionsChanged = isPermissionChanged };
            }
        }

        public BaseResult DeleteCompanyRole(DeleteArg arg)
        {
            var role = Db.GetById<Role>(arg.Id);
            if (role == null)
                return new AddUpdateEntityResult { ErrorMessage = Messages.RoleNotFound };
            if (role.RoleType != (int)SystemRoles.None)
                return new AddUpdateEntityResult { ErrorMessage = Messages.RoleDisabledToDelete };
            if (Db.ExistsUsersLinkedToRole(arg.Id))
                return new AddUpdateEntityResult { ErrorMessage = Messages.RoleDisabledToDelete };
            using (var transaction  = new TransactionScope())
            {
                foreach (var componentsToRole in Db.GetComponentRoles(role.RoleId))
                {
                    Db.Delete(componentsToRole, arg.Reason);
                }
                Db.Delete(role, arg.Reason);
                transaction.Complete();
                return new BaseResult();
            }
        }
        #endregion
    }
}
