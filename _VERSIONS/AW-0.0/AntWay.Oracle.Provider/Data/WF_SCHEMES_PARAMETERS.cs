namespace AntWay.Oracle.Provider.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCMNGR.WF_SCHEMES_PARAMETERS")]
    public partial class WF_SCHEMES_PARAMETERS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WF_SCHEMES_PARAMETERS()
        {
            WF_SCHEME_PARAMETERS_VALUES = new HashSet<WF_SCHEME_PARAMETERS_VALUES>();
        }

        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string SCHEME_CODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string SCHEME_PARAMETER { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WF_SCHEME_PARAMETERS_VALUES> WF_SCHEME_PARAMETERS_VALUES { get; set; }
    }
}
