namespace OnlineBanking.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "FromId", "dbo.BankAccounts");
            DropForeignKey("dbo.Transactions", "ToId", "dbo.BankAccounts");
            DropIndex("dbo.Transactions", new[] { "FromId" });
            DropIndex("dbo.Transactions", new[] { "ToId" });
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
            
            AddColumn("dbo.Cheques", "NumberId", c => c.String());
            AlterColumn("dbo.Accounts", "RoleId", c => c.Int(nullable: false));
            AlterColumn("dbo.BankAccounts", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.BankAccounts", "CurrencyId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "Amount", c => c.Single(nullable: false));
            AlterColumn("dbo.Cheques", "FromBankAccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "ChequeBookId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "ToBankAccountId", c => c.Int());
            AlterColumn("dbo.ChequeBooks", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Channels", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.Messages", "ChannelId", c => c.Int(nullable: false));
            AlterColumn("dbo.Messages", "AccountId", c => c.Int(nullable: false));
            DropColumn("dbo.Cheques", "Address");
            DropColumn("dbo.Transactions", "FromId");
            DropColumn("dbo.Transactions", "ToId");
            DropColumn("dbo.Transactions", "BalancedTo");
            DropColumn("dbo.Transactions", "BalancedFrom");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Transactions", "BalancedFrom", c => c.Double(nullable: false));
            AddColumn("dbo.Transactions", "BalancedTo", c => c.Double(nullable: false));
            AddColumn("dbo.Transactions", "ToId", c => c.Int(nullable: false));
            AddColumn("dbo.Transactions", "FromId", c => c.Int(nullable: false));
            AddColumn("dbo.Cheques", "Address", c => c.String());
            DropForeignKey("dbo.Notifications", "AccountId", "dbo.Accounts");
            DropForeignKey("dbo.TransactionDetails", "TransactionId", "dbo.Transactions");
            DropForeignKey("dbo.TransactionDetails", "BankAccountId", "dbo.BankAccounts");
            DropIndex("dbo.Notifications", new[] { "AccountId" });
            DropIndex("dbo.TransactionDetails", new[] { "TransactionId" });
            DropIndex("dbo.TransactionDetails", new[] { "BankAccountId" });
            AlterColumn("dbo.Messages", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Messages", "ChannelId", c => c.Int(nullable: false));
            AlterColumn("dbo.Channels", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.ChequeBooks", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "ToBankAccountId", c => c.Int());
            AlterColumn("dbo.Cheques", "ChequeBookId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "FromBankAccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "Amount", c => c.Single());
            AlterColumn("dbo.BankAccounts", "CurrencyId", c => c.Int(nullable: false));
            AlterColumn("dbo.BankAccounts", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Accounts", "RoleId", c => c.Int(nullable: false));
            DropColumn("dbo.Cheques", "NumberId");
            DropTable("dbo.Notifications");
            DropTable("dbo.TransactionDetails");
            CreateIndex("dbo.Transactions", "ToId");
            CreateIndex("dbo.Transactions", "FromId");
            AddForeignKey("dbo.Transactions", "ToId", "dbo.BankAccounts", "BankAccountId");
            AddForeignKey("dbo.Transactions", "FromId", "dbo.BankAccounts", "BankAccountId");
        }
    }
}
