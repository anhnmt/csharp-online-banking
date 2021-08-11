namespace OnlineBanking.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Accounts", "RoleId", c => c.Int(nullable: false));
            AlterColumn("dbo.BankAccounts", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.BankAccounts", "CurrencyId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "FromBankAccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "ChequeBookId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "ToBankAccountId", c => c.Int());
            AlterColumn("dbo.ChequeBooks", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Transactions", "FromId", c => c.Int(nullable: false));
            AlterColumn("dbo.Transactions", "ToId", c => c.Int(nullable: false));
            AlterColumn("dbo.Channels", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.Messages", "ChannelId", c => c.Int(nullable: false));
            AlterColumn("dbo.Messages", "AccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Messages", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Messages", "ChannelId", c => c.Int(nullable: false));
            AlterColumn("dbo.Channels", "UserId", c => c.Int(nullable: false));
            AlterColumn("dbo.Transactions", "ToId", c => c.Int(nullable: false));
            AlterColumn("dbo.Transactions", "FromId", c => c.Int(nullable: false));
            AlterColumn("dbo.ChequeBooks", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "ToBankAccountId", c => c.Int());
            AlterColumn("dbo.Cheques", "ChequeBookId", c => c.Int(nullable: false));
            AlterColumn("dbo.Cheques", "FromBankAccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.BankAccounts", "CurrencyId", c => c.Int(nullable: false));
            AlterColumn("dbo.BankAccounts", "AccountId", c => c.Int(nullable: false));
            AlterColumn("dbo.Accounts", "RoleId", c => c.Int(nullable: false));
        }
    }
}
