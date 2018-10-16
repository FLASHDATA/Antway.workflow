namespace AntWay.Oracle.Provider.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCMNGR.WF_SCHEME_PARAMETERS_VALUES")]
    public partial class WF_SCHEME_PARAMETERS_VALUES
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string SCHEME_CODE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string SCHEME_PARAMETER { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(50)]
        public string VALUE { get; set; }

        public virtual WF_SCHEMES_PARAMETERS WF_SCHEMES_PARAMETERS { get; set; }
    }
}
