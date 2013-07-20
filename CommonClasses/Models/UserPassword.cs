using System.ComponentModel.DataAnnotations;

namespace CommonClasses.Models
{
    public class UserPassword
    {
        public bool OldPasswordNeeded { get; set; }

        public string OldPassword { get; set; }

        [Required(ErrorMessage = Messages.PasswordRequired)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = Messages.ConfirmPasswordRequired)]
        [Compare("Password", ErrorMessage = Messages.ConfirmPasswordDoNotMatch)]
        public string ConfirmPassword { get; set; }

        [Required]
        public string UserName { get; set; }

        public string Code { get; set; }
        public string Salt { get; set; }

        public UserPassword() { }
        public UserPassword(string userName, bool oldPasswordNeeded, string code = null)
        {
            UserName = userName;
            OldPasswordNeeded = oldPasswordNeeded;
            Code = code;
        }
    }
}