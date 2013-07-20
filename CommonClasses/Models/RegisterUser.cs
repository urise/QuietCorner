using System.ComponentModel.DataAnnotations;

namespace CommonClasses.Models
{
    public class RegisterUser
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = Messages.LoginRequired)]
        public string Login { get; set; }

        [Required(ErrorMessage = Messages.PasswordRequired)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = Messages.ConfirmPasswordRequired)]
        [Compare("Password", ErrorMessage = Messages.ConfirmPasswordDoNotMatch)]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = Messages.EmailRequired)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(RegexExpressions.EmailRegex, ErrorMessage = Messages.WrondEmailFormat)]
        public string Email { get; set; }
        public string UserFio { get; set; }
        public string RegistrationCode { get; set; }
    }
}