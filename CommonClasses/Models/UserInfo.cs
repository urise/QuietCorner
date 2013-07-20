using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommonClasses.InfoClasses;

namespace CommonClasses.Models
{
    public class UserInfo
    {
        public string Login { get; set; }
        [Required(ErrorMessage = Messages.EmailRequired)]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(RegexExpressions.EmailRegex, ErrorMessage = Messages.WrondEmailFormat)]
        public string Email { get; set; }
        public string UserFio { get; set; }
        public List<RoleInfo> UserRoles { get; set; }
    }
}