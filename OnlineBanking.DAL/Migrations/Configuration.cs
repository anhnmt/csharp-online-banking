
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace OnlineBanking.DAL.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<OnlineBanking.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    } 
}