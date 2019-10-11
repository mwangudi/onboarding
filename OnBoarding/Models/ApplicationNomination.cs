namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ApplicationNomination
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        public int NomineeId { get; set; }

        public int NominationType { get; set; }

        public int NominationStatus { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; }

        public virtual tblStatus tblStatus { get; set; }
    }
}