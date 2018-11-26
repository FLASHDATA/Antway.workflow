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
        public virtual DbSet<WF_SCHEME_PARAMETERS_VALUES> WF_SCHEME_PARAMETERS_VALUES { get; set; }
        public virtual DbSet<WF_SCHEMES> WF_SCHEMES { get; set; }
        public virtual DbSet<WF_SCHEMES_PARAMETERS> WF_SCHEMES_PARAMETERS { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WF_SCHEMES>()
                .Property(e => e.WORKFLOW_SERVICE)
                .HasPrecision(29, 0);

            modelBuilder.Entity<WF_SCHEMES>()
                .Property(e => e.ACTIVE)
                .HasPrecision(29, 0);

            modelBuilder.Entity<WF_SCHEMES_PARAMETERS>()
                .HasMany(e => e.WF_SCHEME_PARAMETERS_VALUES)
                .WithRequired(e => e.WF_SCHEMES_PARAMETERS)
                .HasForeignKey(e => new { e.SCHEME_CODE, e.SCHEME_PARAMETER })
                .WillCascadeOnDelete(false);
        }
    }
}
