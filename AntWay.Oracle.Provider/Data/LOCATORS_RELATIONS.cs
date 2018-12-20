namespace AntWay.Oracle.Provider.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCMNGR.LOCATORS_RELATIONS")]
    public partial class LOCATORS_RELATIONS
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(50)]
        public string ENTITY { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string ENTITY_VALUE { get; set; }

        public Guid? ID_WFPROCESSINSTANCE { get; set; }
    }
}
