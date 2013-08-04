using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses.DbClasses
{
    public class Activity: IMapping, IConstraintedByInstanceId
    {
        public int ActivityId { get; set; }
        public int InstanceId { get; set; }
        [Required, MaxLength(128)]
        public string ActivityName { get; set; }
        
        public bool IsActive { get; set; }
        public int Color { get; set; }

        public virtual Instance Instance { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return ActivityId; }
        }

        #endregion
    }
}
