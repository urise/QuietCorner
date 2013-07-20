using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonClasses.DbClasses
{
    public class Component : IMapping
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ComponentId { get; set; }
        [Required, MaxLength(128)]
        public string ComponentName { get; set; }
        public bool IsReadOnlyAccess { get; set; }

        #region IMapping properties

        public int PrimaryKeyValue
        {
            get { return ComponentId; }
        }

        #endregion
    }
}
