namespace OnlineBanking.DAL.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<OnlineBanking.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "OnlineBanking.DAL.ApplicationDbContext";
        }

        protected override void Seed(OnlineBanking.DAL.ApplicationDbContext context)
        {
            context.Roles.AddOrUpdate(x => x.RoleId,
                new Roles()
                {
                    RoleId = 1,
                    Name = "admin",
                    Status = 1
                },
                new Roles()
                {
                    RoleId = 2,
                    Name = "telesupport",
                    Status = 1
                },
                new Roles()
                {
                    RoleId = 3,
                    Name = "user",
                    Status = 1
                }
                );

            context.Accounts.AddOrUpdate(x => x.AccountId,
                new Accounts() { AccountId = 1, Name = "Admin", Email = "admin@gmail.com", Password = "123456", RoleId = 1, Status = 1 },
                new Accounts() { AccountId = 2, Name = "Telesupport 1", Email = "tele@gmail.com", Password = "123456", RoleId = 2, Status = 1 },
                new Accounts() { AccountId = 3, Name = "Telesupport 2", Email = "tele2@gmail.com", Password = "123456", RoleId = 2, Status = 1 },
                new Accounts() { AccountId = 4, Name = "User 1", Email = "user@gmail.com", Password = "123456", RoleId = 3, Status = 1 },
                new Accounts() { AccountId = 5, Name = "User 2", Email = "user2@gmail.com", Password = "123456", RoleId = 3, Status = 1 }
            );

            context.Currencies.AddOrUpdate(x => x.CurrencyId,
                new Currencies() { CurrencyId = 1, Name = "VND", Status = 1 },
                new Currencies() { CurrencyId = 2, Name = "USD", Status = 1 },
                new Currencies() { CurrencyId = 3, Name = "EUR", Status = 1 }
            );


        }
    }
}
