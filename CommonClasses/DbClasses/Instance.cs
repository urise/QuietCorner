using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses.DbClasses
{
    public class Instance : IMapping
    {
        public int InstanceId { get; set; }
        [MaxLength(256)]
        public string InstanceName { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return InstanceId; }
        }

        #endregion
    }
}
