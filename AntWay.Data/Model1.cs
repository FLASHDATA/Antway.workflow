namespace AntWay.Data
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=AntWayDatabaseModel")
        {
        }

        public virtual DbSet<AW_WF_LOCATOR> AW_WF_LOCATOR { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
