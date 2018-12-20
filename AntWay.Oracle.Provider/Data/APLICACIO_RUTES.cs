namespace AntWay.Oracle.Provider.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PROCMNGR.APLICACIO_RUTES")]
    public partial class APLICACIO_RUTES
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ID { get; set; }

        [StringLength(50)]
        public string NOM_APLICACIO { get; set; }

        [StringLength(150)]
        public string RUTA_APLICACIO { get; set; }

        public decimal TIPUS { get; set; }
    }
}
