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
                        Name = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Password = c.String(nullable: false, maxLength: 24),
                        Phone = c.String(nullable: false),
                        Address = c.String(),
                        Birthday = c.DateTime(),
                        Status = c.Int(),
                        RoleId = c.Int(nullable: false),
                        NumberId = c.String(nullable: false),
                        AttemptLogin = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.AccountId)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
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
                        Name = c.String(nullable: false),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CurrencyId);
            
            CreateTable(
                "dbo.Cheques",
                c => new
                    {
                        ChequeId = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        NumberId = c.String(),
                        Status = c.Int(nullable: false),
                        Amount = c.Single(nullable: false),
                        FromBankAccountId = c.Int(nullable: false),
                        ChequeBookId = c.Int(nullable: false),
                        ToBankAccountId = c.Int(),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.ChequeId)
                .ForeignKey("dbo.ChequeBooks", t => t.ChequeBookId, cascadeDelete: true)
                .ForeignKey("dbo.BankAccounts", t => t.FromBankAccountId)
                .ForeignKey("dbo.BankAccounts", t => t.ToBankAccountId)
                .Index(t => t.ChequeBookId)
                .Index(t => t.FromBankAccountId)
                .Index(t => t.ToBankAccountId);
            
            CreateTable(
                "dbo.ChequeBooks",
                c => new
                    {
                        ChequeBookId = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        AccountId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.ChequeBookId)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
            CreateTable(
                "dbo.TransactionDetails",
                c => new
                    {
                        TransactionDetailId = c.Int(nullable: false, identity: true),
                        TransactionId = c.Int(nullable: false),
                        BankAccountId = c.Int(nullable: false),
                        Balance = c.Double(nullable: false),
                        Type = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.TransactionDetailId)
                .ForeignKey("dbo.BankAccounts", t => t.BankAccountId)
                .ForeignKey("dbo.Transactions", t => t.TransactionId)
                .Index(t => t.BankAccountId)
                .Index(t => t.TransactionId);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        TransactionId = c.Int(nullable: false, identity: true),
                        Status = c.Int(nullable: false),
                        Amount = c.Double(nullable: false),
                        Messages = c.String(),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.TransactionId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RoleId);
            
            CreateTable(
                "dbo.Channels",
                c => new
                    {
                        ChannelId = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.ChannelId)
                .ForeignKey("dbo.Accounts", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        MessageId = c.Int(nullable: false, identity: true),
                        ChannelId = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        Content = c.String(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.MessageId)
                .ForeignKey("dbo.Accounts", t => t.AccountId)
                .ForeignKey("dbo.Channels", t => t.ChannelId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.ChannelId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        AccountId = c.Int(nullable: false),
                        Content = c.String(nullable: false),
                        Status = c.Int(nullable: false),
                        PkType = c.Int(nullable: false),
                        PkId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Accounts", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Channels", "UserId", "dbo.Accounts");
            DropForeignKey("dbo.Messages", "ChannelId", "dbo.Channels");
            DropForeignKey("dbo.Messages", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.Accounts", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.TransactionDetails", "TransactionId", "dbo.Transactions");
            DropForeignKey("dbo.TransactionDetails", "BankAccountId", "dbo.BankAccounts");
            DropForeignKey("dbo.Cheques", "ToBankAccountId", "dbo.BankAccounts");
            DropForeignKey("dbo.Cheques", "FromBankAccountId", "dbo.BankAccounts");
            DropForeignKey("dbo.Cheques", "ChequeBookId", "dbo.ChequeBooks");
            DropForeignKey("dbo.ChequeBooks", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.BankAccounts", "CurrencyId", "dbo.Currencies");
            DropForeignKey("dbo.BankAccounts", "AccountId", "dbo.Accounts");
            DropIndex("dbo.Notifications", new[] { "AccountId" });
            DropIndex("dbo.Channels", new[] { "UserId" });
            DropIndex("dbo.Messages", new[] { "ChannelId" });
            DropIndex("dbo.Messages", new[] { "AccountId" });
            DropIndex("dbo.Accounts", new[] { "RoleId" });
            DropIndex("dbo.TransactionDetails", new[] { "TransactionId" });
            DropIndex("dbo.TransactionDetails", new[] { "BankAccountId" });
            DropIndex("dbo.Cheques", new[] { "ToBankAccountId" });
            DropIndex("dbo.Cheques", new[] { "FromBankAccountId" });
            DropIndex("dbo.Cheques", new[] { "ChequeBookId" });
            DropIndex("dbo.ChequeBooks", new[] { "AccountId" });
            DropIndex("dbo.BankAccounts", new[] { "CurrencyId" });
            DropIndex("dbo.BankAccounts", new[] { "AccountId" });
            DropTable("dbo.Notifications");
            DropTable("dbo.Messages");
            DropTable("dbo.Channels");
            DropTable("dbo.Roles");
            DropTable("dbo.Transactions");
            DropTable("dbo.TransactionDetails");
            DropTable("dbo.ChequeBooks");
            DropTable("dbo.Cheques");
            DropTable("dbo.Currencies");
            DropTable("dbo.BankAccounts");
            DropTable("dbo.Accounts");
        }
    }
}
