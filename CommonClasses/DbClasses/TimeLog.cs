using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses.DbClasses
{
    public class TimeLog: IMapping, IConstraintedByInstanceId
    {
        public int TimeLogId { get; set; }
        public int InstanceId { get; set; }
        public int UserId { get; set; }
        public int ActivityId { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }

        public virtual Instance Instance { get; set; }
        public virtual User User { get; set; }
        public virtual Activity Activity { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return TimeLogId; }
        }

        #endregion
    }
}
