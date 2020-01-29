namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Currency
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string CurrencyShort { get; set; }

        [StringLength(150)]
        public string CurrencyName { get; set; }

        public int Status { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        [StringLength(128)]
        public string CreatedBy { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual tblStatus tblStatus { get; set; }
    }
}
