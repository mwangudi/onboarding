namespace OnBoarding.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DesignatedUser
    {
        public int Id { get; set; }

        public string Surname { get; set; }

        public string Othernames { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DOB { get; set; }

        public string TradingLimit { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        [StringLength(50)]
        public string Telephone { get; set; }

        public bool? EMarketSignUp { get; set; }

        public bool? AcceptedTerms { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        public string Signature { get; set; }

        public int ClientID { get; set; }

        public int? CompanyID { get; set; }

        public string POBox { get; set; }

        public string ZipCode { get; set; }

        [StringLength(50)]
        public string Town { get; set; }

        public int Status { get; set; }

        public string OTP { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [StringLength(128)]
        public string UserAccountID { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? ConfirmationDate { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual RegisteredClient RegisteredClient { get; set; }

        public virtual ClientCompany ClientCompany { get; set; }

        public virtual tblStatus tblStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DesignatedUserApproval> DesignatedUserApprovals { get; set; }
    }
}
