namespace OnBoarding.Models
{
    using System.Data.Entity;

    public partial class DBModel : DbContext
    {
        public DBModel() : base("name=DBModel") {}

        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
        public virtual DbSet<ClientSettlementAccount> ClientSettlementAccounts { get; set; }
        public virtual DbSet<ClientSignatory> ClientSignatories { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<DesignatedUser> DesignatedUsers { get; set; }
        public virtual DbSet<EMarketApplication> EMarketApplications { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<RegisteredClient> RegisteredClients { get; set; }
        public virtual DbSet<SignatoryApproval> SignatoryApprovals { get; set; }
        public virtual DbSet<DesignatedUserApproval> DesignatedUserApprovals { get; set; }
        public virtual DbSet<tblStatus> tblStatus { get; set; }
        public virtual DbSet<SystemMenu> SystemMenus { get; set; }
        public virtual DbSet<SystemMenuAccess> SystemMenuAccess { get; set; }
        public virtual DbSet<AuditTrail> AuditTrails { get; set; }
        public virtual DbSet<ClientCompany> ClientCompanies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.ClientSignatories)
                .WithOptional(e => e.AspNetUser)
                .HasForeignKey(e => e.UserAccountID);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.DesignatedUsers)
                .WithOptional(e => e.AspNetUser)
                .HasForeignKey(e => e.UserAccountID);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.Currencies)
                .WithOptional(e => e.AspNetUser)
                .HasForeignKey(e => e.CreatedBy);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.EMarketApplications)
                .WithOptional(e => e.AspNetUser)
                .HasForeignKey(e => e.OPSWhoApproved);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.EMarketApplications)
                .WithOptional(e => e.AspNetUser)
                .HasForeignKey(e => e.POAWhoApproved);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.RegisteredClients)
                .WithOptional(e => e.AspNetUser)
                .HasForeignKey(e => e.UserAccountID);

            modelBuilder.Entity<AspNetUser>()
               .HasMany(e => e.ExistingClientsUploads)
               .WithOptional(e => e.AspNetUser)
               .HasForeignKey(e => e.UploadedBy);

            modelBuilder.Entity<AspNetUser>()
               .HasMany(e => e.ExistingClientsUploads)
               .WithOptional(e => e.AspNetUser)
               .HasForeignKey(e => e.ApprovedBy);

            modelBuilder.Entity<ClientSignatory>()
                .Property(e => e.Signature)
                .IsUnicode(false);

            modelBuilder.Entity<ClientSignatory>()
                .HasMany(e => e.SignatoryApprovals)
                .WithRequired(e => e.ClientSignatory)
                .HasForeignKey(e => e.SignatoryID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<DesignatedUser>()
               .HasMany(e => e.DesignatedUserApprovals)
               .WithRequired(e => e.DesignatedUser)
               .HasForeignKey(e => e.UserID)
               .WillCascadeOnDelete(true);

            modelBuilder.Entity<EMarketApplication>()
                .HasMany(e => e.SignatoryApprovals)
                .WithRequired(e => e.EMarketApplication)
                .HasForeignKey(e => e.ApplicationID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<EMarketApplication>()
                .HasMany(e => e.DesignatedUserApprovals)
                .WithRequired(e => e.EMarketApplication)
                .HasForeignKey(e => e.ApplicationID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RegisteredClient>()
                .HasMany(e => e.ClientSettlementAccounts)
                .WithRequired(e => e.RegisteredClient)
                .HasForeignKey(e => e.ClientID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<RegisteredClient>()
                .HasMany(e => e.ClientSignatories)
                .WithOptional(e => e.RegisteredClient)
                .HasForeignKey(e => e.ClientID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<RegisteredClient>()
                .HasMany(e => e.DesignatedUsers)
                .WithRequired(e => e.RegisteredClient)
                .HasForeignKey(e => e.ClientID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<RegisteredClient>()
                .HasMany(e => e.EMarketApplications)
                .WithRequired(e => e.RegisteredClient)
                .HasForeignKey(e => e.ClientID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClientCompany>()
                .HasMany(e => e.EMarketApplications)
                .WithRequired(e => e.ClientCompany)
                .HasForeignKey(e => e.CompanyID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClientCompany>()
              .HasMany(e => e.ClientSignatories)
              .WithRequired(e => e.ClientCompany)
              .HasForeignKey(e => e.CompanyID)
              .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClientCompany>()
               .HasMany(e => e.DesignatedUsers)
               .WithRequired(e => e.ClientCompany)
               .HasForeignKey(e => e.CompanyID)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<ClientCompany>()
              .HasMany(e => e.ClientSettlementAccounts)
              .WithRequired(e => e.ClientCompany)
              .HasForeignKey(e => e.CompanyID)
              .WillCascadeOnDelete(false);

            modelBuilder.Entity<RegisteredClient>()
                .HasMany(e => e.ClientCompanies)
                .WithRequired(e => e.RegisteredClient)
                .HasForeignKey(e => e.ClientId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.ClientSettlementAccounts)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.ClientSignatories)
                .WithOptional(e => e.tblStatus)
                .HasForeignKey(e => e.Status);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.Currencies)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.DesignatedUsers)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.EMarketApplications)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.RegisteredClients)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.SignatoryApprovals)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.DesignatedUserApprovals)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
                .HasMany(e => e.AspNetUsers)
                .WithRequired(e => e.tblStatus)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
               .HasMany(e => e.ClientCompanies)
               .WithRequired(e => e.tblStatus)
               .HasForeignKey(e => e.Status)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblStatus>()
               .HasMany(e => e.ExistingClientsUploads)
               .WithRequired(e => e.tblStatus)
               .HasForeignKey(e => e.Status)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<SystemMenuAccess>().ToTable("SystemMenuAccess");
        }
    }
}
