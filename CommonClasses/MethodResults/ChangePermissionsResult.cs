using CommonClasses.Roles;

namespace CommonClasses.MethodResults
{
    public class ChangePermissionsResult : BaseResult
    {
        public int EntityId { get; set; }
        public bool IsPermissionsChanged { get; set; }
        public UserAccess CurrentUserPermissions { get; set; }
    }
}