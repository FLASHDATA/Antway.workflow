namespace AntWay.Oracle.Provider.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCMNGR.WF_SCHEMES")]
    public partial class WF_SCHEMES
    {
        [Key]
        [StringLength(50)]
        public string SCHEME_CODE { get; set; }

        [StringLength(50)]
        public string SCHEME_NAME { get; set; }
        
        [StringLength(50)]
        public string DB_SCHEME_NAME { get; set; }

        [StringLength(50)]
        public string DESCRIPTION { get; set; }

        public decimal WORKFLOW_SERVICE { get; set; }

        public decimal ACTIVE { get; set; }

    }
}
