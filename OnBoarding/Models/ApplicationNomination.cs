namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ApplicationNomination
    {
        public int Id { get; set; }
        public int ApplicationID { get; set; }
        public int ClientID { get; set; }
        public int CompanyID { get; set; }
        public string NomineeEmail { get; set; }
        public int NominationType { get; set; }
        public int NominationStatus { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; }
    }
}