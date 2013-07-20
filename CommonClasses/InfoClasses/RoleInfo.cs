using Interfaces.Enums;

namespace CommonClasses.InfoClasses
{
    public class RoleInfo
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public SystemRoles Type { get; set; }
        public bool IsUsed { get; set; }
        public bool IsReadOnly { get; set; }
    }
}