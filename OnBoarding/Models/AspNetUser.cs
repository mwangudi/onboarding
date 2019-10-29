namespace OnBoarding.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AspNetUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AspNetUser()
        {
            ClientSignatories = new HashSet<ClientSignatory>();
            Currencies = new HashSet<Currency>();
            EMarketApplications = new HashSet<EMarketApplication>();
            RegisteredClients = new HashSet<RegisteredClient>();
            ExistingClientsUploads = new HashSet<ExistingClientsUpload>();
        }

        public string Id { get; set; }

        [StringLength(256)]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public string PasswordHash { get; set; }

        public string SecurityStamp { get; set; }

        public string PhoneNumber { get; set; }

        public bool PhoneNumberConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTime? LockoutEndDateUtc { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        public int Status { get; set; }

        public string StaffNumber { get; set; }

        public string PasswordResetCode { get; set; }

        [Required]
        [StringLength(256)]
        public string UserName { get; set; }

        [StringLength(256)]
        public string CompanyName { get; set; }

        [Column(TypeName = "datetime2")]

        public DateTime DateCreated { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime LastPasswordChangedDate { get; set; } = DateTime.Now;

        public virtual tblStatus tblStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSignatory> ClientSignatories { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DesignatedUser> DesignatedUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Currency> Currencies { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMarketApplication> EMarketApplications { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RegisteredClient> RegisteredClients { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExistingClientsUpload> ExistingClientsUploads { get; set; }
    }
}
