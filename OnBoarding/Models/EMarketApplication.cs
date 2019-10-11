namespace OnBoarding.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class EMarketApplication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EMarketApplication()
        {
            SignatoryApprovals = new HashSet<SignatoryApproval>();
        }

        public int Id { get; set; }

        public int? ClientID { get; set; }

        public int? CompanyID { get; set; }

        public bool AcceptedTAC { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public int DesignatedUsers { get; set; }

        public int UsersApproved { get; set; }
        public DateTime? UsersDateApproved { get; set; }

        public int Signatories { get; set; }
        public int SignatoriesApproved { get; set; }
        public DateTime? SignatoriesDateApproved { get; set; }

        [StringLength(128)]
        public string OPSWhoApproved { get; set; }
        public string OPSWhoDeclined { get; set; }
        public bool OPSApproved { get; set; }
        public bool OPSDeclined { get; set; }
        public DateTime? OPSDateApproved { get; set; }
        public int OPSApprovalStatus { get; set; }
        public string OPSComments { get; set; }

        [StringLength(128)]
        public string POAWhoApproved { get; set; }
        public string POAWhoDeclined { get; set; }
        public bool POAApproved { get; set; }
        public bool POADeclined { get; set; }
        public DateTime? POADateApproved { get; set; }
        public int POAApprovalStatus { get; set; }
        public string POAComments { get; set; }

        [StringLength(128)]
        public string DigWhoDeclined { get; set; }
        public bool DigitalDeskDeclined { get; set; }
        public DateTime? DateDeclined { get; set; }

        public int Status { get; set; }

        public bool? Emt { get; set; }

        public bool? SSI { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual RegisteredClient RegisteredClient { get; set; }
        public virtual ClientCompany ClientCompany { get; set; }

        public virtual tblStatus tblStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SignatoryApproval> SignatoryApprovals { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DesignatedUserApproval> DesignatedUserApprovals { get; set; }
    }
}
