namespace CommonClasses.DbClasses
{
    public class ComponentRole : IMapping, IConstraintedByInstanceId
    {
        public int ComponentRoleId { get; set; }
        public int ComponentId { get; set; }
        public int RoleId { get; set; }
        public AccessLevel AccessLevel { get; set; }

        #region IConstraintedByInstanceId

        public int InstanceId { get; set; }

        #endregion

        public virtual Component Component { get; set; }
        public virtual Role Role { get; set; }
        public virtual Instance Instance { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return ComponentRoleId; }
        }

        #endregion
    }
}