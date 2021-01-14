using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MySQL.Data;
using MySQL.Data.EntityFrameworkCore;

namespace F15.Database
{
    class XPContext : DbContext
    {
        public DbSet<Xp> Xp { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL("server=baloegwoaygpxdwfnluc-mysql.services.clever-cloud.com;database=baloegwoaygpxdwfnluc;user=uiqlndsekur9x4ng;password=YyDhKh6qnhQIZrwaK4DN");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
