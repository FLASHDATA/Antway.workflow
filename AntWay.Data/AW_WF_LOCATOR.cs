namespace AntWay.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCMNGR.AW_WF_LOCATOR")]
    public partial class AW_WF_LOCATOR
    {
        [Required]
        public Guid ID_WFPROCESSINSTANCE { get; set; }


        [StringLength(50)]
        public string LOCATOR_FIELD_NAME { get; set; }

        [Key]
        [StringLength(50)]
        public string LOCATOR_VALUE { get; set; }

        [StringLength(50)]
        public string ALTERN_LOCATOR_FIELD_NAME1 { get; set; }

        [StringLength(50)]
        public string ALTERN_LOCATOR_FIELD_VALUE1 { get; set; }
    }
}
