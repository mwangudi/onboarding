namespace OnBoarding.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class RegisteredClient
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RegisteredClient()
        {
            ClientSettlementAccounts = new HashSet<ClientSettlementAccount>();
            ClientSignatories = new HashSet<ClientSignatory>();
            DesignatedUsers = new HashSet<DesignatedUser>();
            EMarketApplications = new HashSet<EMarketApplication>();
            ClientCompanies = new HashSet<ClientCompany>();
        }

        public int Id { get; set; }
        public string Surname { get; set; }
        public string OtherNames { get; set; }
        public string IDRegNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string PostalAddress { get; set; }
        public string PostalCode { get; set; }
        public string TownCity { get; set; }
        public string Fax { get; set; }
        public int Status { get; set; }
        public string OTP { get; set; }
        public DateTime? OTPDateCreated { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? RegistrationDate { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? RegistrationConfirmationDate { get; set; }
        public bool? AcceptedTerms { get; set; }
        public bool? AcceptedUserTerms { get; set; }
        [StringLength(128)]
        public string UserAccountID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; }
        public string Signature { get; set; }
        public string UploadedBy { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSettlementAccount> ClientSettlementAccounts { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSignatory> ClientSignatories { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DesignatedUser> DesignatedUsers { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMarketApplication> EMarketApplications { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientCompany> ClientCompanies { get; set; }

        public virtual tblStatus tblStatus { get; set; }
    }
}
