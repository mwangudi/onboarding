namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ExistingClientsUpload
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEmail { get; set; }
        public string AcceptedTerms { get; set; }
        public string EMTSignUp { get; set; }
        public string SSI { get; set; }
        public string AccountNumber { get; set; }
        public string Currency { get; set; }
        public string RepresentativeName { get; set; }
        public string RepresentativeEmail { get; set; }
        public string RepresentativePhonenumber { get; set; }
        public string RepresentativeLimit { get; set; }
        public string IsGM { get; set; }
        public string IsEMTUser { get; set; }
        public string DateOfContract { get; set; }
        public int Status { get; set; }

        public string FileName { get; set; }

        [StringLength(128)]
        public string UploadedBy { get; set; }
        public string ErrorComments { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [StringLength(128)]
        public string ApprovedBy { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime? DateApproved { get; set; } = DateTime.Now;

        public virtual tblStatus tblStatus { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
    }
}