namespace OnlineBanking.DAL.Migrations
{
    using OnlineBanking.DAL;
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

        //protected override void Seed(OnlineBanking.DAL.ApplicationDbContext context)
        //{
        //    context.Roles.AddOrUpdate(x => x.RoleId,
        //        new Roles()
        //        {
        //            RoleId = 1,
        //            Name = "Admin",
        //            Status = ((int) DefaultStatus.Actived)
        //        },
        //        new Roles()
        //        {
        //            RoleId = 2,
        //            Name = "Support",
        //            Status = ((int) DefaultStatus.Actived)
        //        },
        //        new Roles()
        //        {
        //            RoleId = 3,
        //            Name = "User",
        //            Status = ((int) DefaultStatus.Actived)
        //        }
        //    );

        //    context.Accounts.AddOrUpdate(x => x.AccountId,
        //        new Accounts()
        //        {
        //            AccountId = 1,
        //            Name = "Administrator",
        //            Email = "admin@gmail.com",
        //            Password = "123456",
        //            RoleId = 1,
        //            Phone = "0973086596",
        //            Address = "Ha noi",
        //            Birthday = DateTime.Parse("1996-07-12"),
        //            NumberId = "125616121",
        //            CreatedAt= DateTime.Parse("2021-07-26"),
        //            Status = ((int) AccountStatus.Actived),
        //            AttemptLogin = 0,
        //        },
        //        new Accounts()
        //        {
        //            AccountId = 2,
        //            Name = "Supporter Trinh Van A",
        //            Email = "support@gmail.com",
        //            Password = "123456",
        //            Phone = "0973086594",
        //            Birthday = DateTime.Parse("1994-02-12"),
        //            Address = "Ha noi",
        //            CreatedAt = DateTime.Parse("2021-07-26"),
        //            NumberId = "125616125",
        //            RoleId = 2,
        //            Status = ((int) AccountStatus.Actived),
        //            AttemptLogin = 0,
        //        },
        //        new Accounts()
        //        {
        //            AccountId = 3,
        //            Name = "Supporter Lo Thi B",
        //            Email = "support2@gmail.com",
        //            Password = "123456",
        //            Phone = "0973086593",
        //            Birthday = DateTime.Parse("1994-03-12"),
        //            Address = "Ha noi",
        //            NumberId = "125616124",
        //            CreatedAt = DateTime.Parse("2021-07-26"),
        //            RoleId = 2,
        //            Status = ((int) AccountStatus.Actived),
        //            AttemptLogin = 0,
        //        },
        //        new Accounts()
        //        {
        //            AccountId = 4,
        //            Name = "Nguyen Van A",
        //            Email = "user@gmail.com",
        //            Password = "123456",
        //            Birthday = DateTime.Parse("1994-03-12"),
        //            Phone = "0973086592",
        //            Address = "Ha noi",
        //            CreatedAt = DateTime.Parse("2021-07-26"),
        //            NumberId = "125616123",
        //            RoleId = 3,
        //            Status = ((int) AccountStatus.Actived),
        //            AttemptLogin = 0,
        //        },
        //        new Accounts()
        //        {
        //            AccountId = 5,
        //            Name = "Ngo Van B",
        //            Email = "user2@gmail.com",
        //            Password = "123456",
        //            Birthday = DateTime.Parse("1998-05-12"),
        //            Phone = "0973086591",
        //            Address = "Ho Chi Minh",
        //            CreatedAt = DateTime.Parse("2021-07-26"),
        //            NumberId = "125616122",
        //            RoleId = 3,
        //            Status = ((int) AccountStatus.Actived),
        //            AttemptLogin = 0,
        //        }
        //    );

        //    context.Currencies.AddOrUpdate(x => x.CurrencyId,
        //        new Currencies()
        //        {
        //            CurrencyId = 1,
        //            Name = "VND",
        //            Status = ((int) DefaultStatus.Actived)
        //        },
        //        new Currencies()
        //        {
        //            CurrencyId = 2,
        //            Name = "USD",
        //            Status = ((int) DefaultStatus.Actived)
        //        },
        //        new Currencies()
        //        {
        //            CurrencyId = 3,
        //            Name = "EUR",
        //            Status = ((int) DefaultStatus.Actived)
        //        }
        //    );
        //}
    }
}