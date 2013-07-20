using System;
using System.ComponentModel.DataAnnotations;

namespace CommonClasses.DbClasses
{
    public class TemporaryCode : IMapping
    {
        public int TemporaryCodeId { get; set; }
        public int UserId { get; set; }
        [Required, MaxLength(10)]
        public string Code { get; set; }
        public DateTime ExpireDate { get; set; }

        public virtual User User { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return TemporaryCodeId; }
        }

        #endregion
    }
}
