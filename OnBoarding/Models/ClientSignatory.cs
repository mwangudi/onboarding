namespace OnBoarding.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ClientSignatory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ClientSignatory()
        {
            SignatoryApprovals = new HashSet<SignatoryApproval>();
        }

        public int Id { get; set; }

        [StringLength(50)]
        public string Surname { get; set; }

        [StringLength(50)]
        public string OtherNames { get; set; }

        [Required]
        [StringLength(50)]
        public string Designation { get; set; }

        [Required]
        [StringLength(50)]
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public int ClientID { get; set; }

        public int CompanyID { get; set; }

        public bool? AcceptedTerms { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; }

        public int? Status { get; set; }

        public string OTP { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ConfirmationDate { get; set; }

        [StringLength(128)]
        public string UserAccountID { get; set; }

        public string Signature { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual RegisteredClient RegisteredClient { get; set; }

        public virtual ClientCompany ClientCompany { get; set; }

        public virtual tblStatus tblStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SignatoryApproval> SignatoryApprovals { get; set; }
    }
}
