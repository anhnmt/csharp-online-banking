﻿using OnlineBanking.DAL.Migrations;
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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Transactions>()
                        .HasRequired(t => t.FromAccount)
                        .WithMany(a => a.FromTransactions)
                        .HasForeignKey(m => m.FromId)
                        .WillCascadeOnDelete(false);

            modelBuilder.Entity<Transactions>()
                        .HasRequired(m => m.ToAccount)
                        .WithMany(a => a.ToTransactions)
                        .HasForeignKey(m => m.ToId)
                        .WillCascadeOnDelete(false);
        }

        public virtual DbSet<Accounts> Accounts { get; set; }
        public virtual DbSet<BankAccounts> BankAccounts { get; set; }
        public virtual DbSet<Currencies> Currencies { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Transactions> Transactions { get; set; }
    }
}