using System.ComponentModel.DataAnnotations;
using Interfaces.Enums;

namespace CommonClasses.DbClasses
{
    public class Role : IMapping, IConstraintedByInstanceId
    {
        public int RoleId { get; set; }
        [Required, MaxLength(128)]
        public string RoleName { get; set; }
        public SystemRoles RoleType { get; set; }

        #region IConstraintedByInstanceId

        public int InstanceId { get; set; }

        #endregion

        public virtual Instance Instance { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return RoleId; }
        }

        #endregion

    }
}
