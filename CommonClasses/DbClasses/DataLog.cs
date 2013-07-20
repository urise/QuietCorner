using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonClasses.DbClasses
{
    public class DataLog: IMapping
    {
        public int DataLogId { get; set; }
        public int? InstanceId { get; set; }
        public int? UserId { get; set; }
        public DateTime OperationTime { get; set; }
        [MaxLength(128)]
        public string TableName { get; set; }
        public int RecordId { get; set; }
        [MaxLength(1)]
        public string Operation { get; set; }
        [Column(TypeName="xml")]
        public string Details { get; set; }
        public int? TransactionNumber { get; set; }

        public virtual Instance Instance { get; set; }
        public virtual User User { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return DataLogId; }
        }

        #endregion
    }
}
