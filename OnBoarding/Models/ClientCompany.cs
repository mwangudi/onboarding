namespace OnBoarding.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ClientCompany
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientCompany()
        {
            EMarketApplications = new HashSet<EMarketApplication>();
            ClientSignatories = new HashSet<ClientSignatory>();
            DesignatedUsers = new HashSet<DesignatedUser>();
        }

        public int Id { get; set; }
        public int ClientId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegNumber { get; set; }
        public string CompanyBuilding { get; set; } 
        public string KRAPin { get; set; }
        public string CompanyStreet { get; set; }
        public string CompanyTownCity { get; set; }
        public string BusinessEmailAddress { get; set; }
        public string AttentionTo { get; set; }
        public string Fax { get; set; }
        public string PostalAddress { get; set; }
        public string PostalCode { get; set; }
        public string TownCity { get; set; }
        public int Status { get; set; }
        public bool HasApplication { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateDeleted { get; set; }

        public string DeletedBy { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EMarketApplication> EMarketApplications { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ClientSignatory> ClientSignatories { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DesignatedUser> DesignatedUsers { get; set; }

        public virtual ICollection<ClientSettlementAccount> ClientSettlementAccounts { get; set; }

        public virtual tblStatus tblStatus { get; set; }

        public virtual RegisteredClient RegisteredClient { get; set; }
         
    }
}