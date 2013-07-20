using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses.DbClasses
{
    public class User : IMapping
    {
        public int UserId { get; set; }
        [Required, MaxLength(128)]
        public string Login { get; set; }
        [MaxLength(1024)]
        public string Password { get; set; }
        [MaxLength(128)]
        public string Email { get; set; }
        [MaxLength(128)]
        public string UserFio { get; set; }
        [MaxLength(10)]
        public string RegistrationCode { get; set; }
        public bool IsActive { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return UserId; }
        }

        #endregion
    }
}
