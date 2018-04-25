 using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkFlow.Data
{
    public partial class Model1 : DbContext
    {
        public void OnModelCreatingExtension(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<EXPEDIENT>()
            //    .Property(e => e.ID)
            //    .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
