using System.Collections.Generic;
using CommonClasses.DbClasses;
using CommonClasses.InfoClasses;
using Interfaces.Enums;

namespace CommonClasses.Models
{
    public class RoleModel
    {
        public RoleModel(Role role)
        {
            RoleId = role.RoleId;
            Name = role.RoleName;
            Type = role.RoleType;
        }

        public RoleModel()
        {
        }

        public int RoleId { get; set; }
        public string Name { get; set; }
        public SystemRoles Type { get; set; }

        public List<ComponentInfo> Components { get; set; }

        public string UserNames { get; set; }

        public bool IsReadOnly
        {
            get { return Type != (int)SystemRoles.None; }
        }
    }
}