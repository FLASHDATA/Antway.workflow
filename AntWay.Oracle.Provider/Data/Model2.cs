namespace AntWay.Oracle.Provider.Data
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

        public virtual DbSet<LOCATORS> LOCATORS { get; set; }
        public virtual DbSet<WF_SCHEMES> WF_SCHEMES { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
