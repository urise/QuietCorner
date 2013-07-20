using System.ComponentModel;

namespace Interfaces.Enums
{
    public enum SystemRoles
    {
        None = 0,
        [Description("Администратор")]
        Administrator = 1,
        [Description("Гость")]
        Guest = 2
    }
}