namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class DesignatedUserApproval
    {
        public int Id { get; set; }

        public int UserID { get; set; }

        public int ApplicationID { get; set; }

        public int Status { get; set; }

        public bool? AcceptedTerms { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateApproved { get; set; }

        public string Comments { get; set; }

        public virtual DesignatedUser DesignatedUser { get; set; }

        public virtual tblStatus tblStatus { get; set; }

        public virtual EMarketApplication EMarketApplication { get; set; }
    }
}