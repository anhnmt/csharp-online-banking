using OnlineBanking.DAL.Migrations;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace OnlineBanking.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("name=DBConnectionString")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        public virtual DbSet<Accounts> Accounts { get; set; }
    }
}