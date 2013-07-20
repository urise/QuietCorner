using CommonClasses.Helpers;

namespace CommonClasses.MethodArguments
{
    public class LogonArg
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public int DefaultInstanceId { get; set; }
        
        public LogonArg()
        {
        }

        public LogonArg(string login, string passwordHash, string salt)
        {
            Login = login;
            PasswordHash = passwordHash;
            Salt = salt;
            DefaultInstanceId = AppConfiguration.DefaultCompanyId;
        }
    }
}