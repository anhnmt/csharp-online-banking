namespace OnlineBanking.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        AccountId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        Phone = c.String(),
                        Address = c.String(),
                        Birthday = c.DateTime(),
                        Status = c.Int(),
                        RoleId = c.Int(),
                        NumberID = c.Int(),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.AccountId)
                .ForeignKey("dbo.Roles", t => t.RoleId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.BankAccounts",
                c => new
                    {
                        BankAccountId = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        CurrencyId = c.Int(nullable: false),
                        Name = c.String(),
                        Balance = c.Double(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.BankAccountId)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .ForeignKey("dbo.Currencies", t => t.CurrencyId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.CurrencyId);
            
            CreateTable(
                "dbo.Currencies",
                c => new
                    {
                        CurrencyId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.CurrencyId);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        TransactionId = c.Int(nullable: false, identity: true),
                        FromId = c.Int(nullable: false),
                        ToId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        Amount = c.Double(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.TransactionId)
                .ForeignKey("dbo.BankAccounts", t => t.FromId)
                .ForeignKey("dbo.BankAccounts", t => t.ToId)
                .Index(t => t.FromId)
                .Index(t => t.ToId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RoleId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Accounts", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Transactions", "ToId", "dbo.BankAccounts");
            DropForeignKey("dbo.Transactions", "FromId", "dbo.BankAccounts");
            DropForeignKey("dbo.BankAccounts", "CurrencyId", "dbo.Currencies");
            DropForeignKey("dbo.BankAccounts", "AccountId", "dbo.Accounts");
            DropIndex("dbo.Accounts", new[] { "RoleId" });
            DropIndex("dbo.Transactions", new[] { "ToId" });
            DropIndex("dbo.Transactions", new[] { "FromId" });
            DropIndex("dbo.BankAccounts", new[] { "CurrencyId" });
            DropIndex("dbo.BankAccounts", new[] { "AccountId" });
            DropTable("dbo.Roles");
            DropTable("dbo.Transactions");
            DropTable("dbo.Currencies");
            DropTable("dbo.BankAccounts");
            DropTable("dbo.Accounts");
        }
    }
}
