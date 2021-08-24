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
        private static ApplicationDbContext _dbContext;

        // lock object
        private static readonly object LockObject = new object();

        public ApplicationDbContext() : base("name=DBConnectionString")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
        }

        // Class in Instance
        public static ApplicationDbContext Instance()
        {
            if (_dbContext == null)
            {
                lock (LockObject)
                {
                    if (_dbContext == null)
                        _dbContext = new ApplicationDbContext();
                }
            }

            return _dbContext;
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

            modelBuilder.Entity<Cheques>()
                .HasRequired(t => t.FromBankAccount)
                .WithMany(a => a.FromCheques)
                .HasForeignKey(m => m.FromBankAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cheques>()
                .HasOptional(t => t.ToBankAccount)
                .WithMany(a => a.ToCheques)
                .HasForeignKey(m => m.ToBankAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Messages>()
                .HasRequired(m => m.Account)
                .WithMany()
                .HasForeignKey(m => m.AccountId)
                .WillCascadeOnDelete(false);
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseModel && (
                    e.State == EntityState.Added
                    || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((BaseModel) entityEntry.Entity).UpdatedAt = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((BaseModel) entityEntry.Entity).CreatedAt = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }

        public virtual DbSet<Accounts> Accounts { get; set; }
        public virtual DbSet<BankAccounts> BankAccounts { get; set; }
        public virtual DbSet<Currencies> Currencies { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<Transactions> Transactions { get; set; }
        public virtual DbSet<Channels> Channels { get; set; }
        public virtual DbSet<Messages> Messages { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
        public virtual DbSet<ChequeBooks> ChequeBooks { get; set; }
        public virtual DbSet<Cheques> Cheques { get; set; }
    }
}