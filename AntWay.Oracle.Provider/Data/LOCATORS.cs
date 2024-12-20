namespace AntWay.Oracle.Provider.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCMNGR.LOCATORS")]
    public partial class LOCATORS
    {
        public Guid ID_WFPROCESSINSTANCE { get; set; }

        [StringLength(50)]
        public string LOCATOR_FIELD_NAME { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string LOCATOR_VALUE { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string SCHEME_CODE { get; set; }
    }
}
