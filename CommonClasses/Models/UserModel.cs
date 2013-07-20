using System.ComponentModel.DataAnnotations;

namespace CommonClasses.Models
{
    public class UserModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = Messages.LoginRequired)]
        public string Login { get; set; }

        [Required(ErrorMessage = Messages.PasswordRequired)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Email { get; set; }
        public string UserFio { get; set; }
        public string RegistrationCode { get; set; }
        public bool IsActive { get; set; }

        public bool KeepLoggedIn { get; set; }
    }
}