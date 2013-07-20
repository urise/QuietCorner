using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses.DbClasses
{
    public class UserInstance : IMapping
    {
        public int UserInstanceId { get; set; }
        public int UserId { get; set; }
        public int InstanceId { get; set; }

        public virtual User User { get; set; }
        public virtual Instance Instance { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return UserInstanceId; }
        }

        #endregion
    }
}
