namespace OnBoarding.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companyname : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClientSettlementAccounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientID = c.Int(nullable: false),
                        CurrencyID = c.Int(nullable: false),
                        AccountNumber = c.String(nullable: false, maxLength: 50),
                        Status = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RegisteredClients", t => t.ClientID)
                .Index(t => t.ClientID);
            
            CreateTable(
                "dbo.RegisteredClients",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountNumber = c.String(nullable: false),
                        AccountName = c.String(maxLength: 50),
                        IncorporationNumber = c.String(maxLength: 50),
                        KRAPin = c.String(maxLength: 50),
                        CompanyName = c.String(nullable: false, maxLength: 50),
                        EmailAddress = c.String(nullable: false),
                        IDRegNumber = c.String(),
                        Status = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        CreatedBy = c.String(),
                        UserAccountID = c.String(maxLength: 128),
                        OTP = c.String(),
                        RegistrationDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        RegistrationConfirmationDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        AcceptedTerms = c.Boolean(),
                        Surname = c.String(maxLength: 50),
                        OtherNames = c.String(maxLength: 50),
                        TownCity = c.String(),
                        Building = c.String(),
                        Street = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ClientSignatories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Surname = c.String(maxLength: 50),
                        OtherNames = c.String(maxLength: 50),
                        Designation = c.String(nullable: false, maxLength: 50),
                        EmailAddress = c.String(nullable: false, maxLength: 50),
                        Signature = c.String(),
                        ClientID = c.Int(),
                        DateCreated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Status = c.Int(),
                        OTP = c.String(),
                        ConfirmationDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        UserAccountID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RegisteredClients", t => t.ClientID)
                .Index(t => t.ClientID);
            
            CreateTable(
                "dbo.DesignatedUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Surname = c.String(),
                        Othernames = c.String(),
                        LegalAddress = c.String(),
                        DOB = c.DateTime(precision: 7, storeType: "datetime2"),
                        TransactionLimit = c.Decimal(precision: 18, scale: 5),
                        Mobile = c.String(maxLength: 50),
                        Telephone = c.String(maxLength: 50),
                        Email = c.String(maxLength: 50),
                        Signature = c.String(),
                        ClientID = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RegisteredClients", t => t.ClientID)
                .Index(t => t.ClientID);
            
            CreateTable(
                "dbo.EMarketApplications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientID = c.Int(nullable: false),
                        DesignatedUsers = c.Int(nullable: false),
                        Signatories = c.Int(nullable: false),
                        SignatoriesApproved = c.Int(nullable: false),
                        AcceptedTAC = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Status = c.Int(nullable: false),
                        Approved = c.Boolean(nullable: false),
                        DateApproved = c.DateTime(precision: 7, storeType: "datetime2"),
                        ApprovedBy = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RegisteredClients", t => t.ClientID)
                .Index(t => t.ClientID);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.String(nullable: false, maxLength: 50),
                        To = c.String(nullable: false, maxLength: 50),
                        From = c.String(nullable: false, maxLength: 50),
                        MessageBody = c.String(nullable: false),
                        DateCreated = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EMarketApplications", "ClientID", "dbo.RegisteredClients");
            DropForeignKey("dbo.DesignatedUsers", "ClientID", "dbo.RegisteredClients");
            DropForeignKey("dbo.ClientSignatories", "ClientID", "dbo.RegisteredClients");
            DropForeignKey("dbo.ClientSettlementAccounts", "ClientID", "dbo.RegisteredClients");
            DropIndex("dbo.EMarketApplications", new[] { "ClientID" });
            DropIndex("dbo.DesignatedUsers", new[] { "ClientID" });
            DropIndex("dbo.ClientSignatories", new[] { "ClientID" });
            DropIndex("dbo.ClientSettlementAccounts", new[] { "ClientID" });
            DropTable("dbo.Notifications");
            DropTable("dbo.EMarketApplications");
            DropTable("dbo.DesignatedUsers");
            DropTable("dbo.ClientSignatories");
            DropTable("dbo.RegisteredClients");
            DropTable("dbo.ClientSettlementAccounts");
        }
    }
}
