namespace CommonClasses.DbClasses
{
    public class UserRole : IMapping, IConstraintedByInstanceId
    {
        public int UserRoleId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }

        #region IConstraintedByInstanceId

        public int InstanceId { get; set; }

        #endregion

        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
        public virtual Instance Instance { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return UserRoleId; }
        }

        #endregion
    }
}