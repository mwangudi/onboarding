namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ClientSettlementAccount
    {
        public int Id { get; set; }

        public int ClientID { get; set; }
        public int? CompanyID { get; set; }

        public int? CurrencyID { get; set; }

        [StringLength(50)]
        public string OtherCurrency { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        public int Status { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public virtual RegisteredClient RegisteredClient { get; set; }

        public virtual tblStatus tblStatus { get; set; }

        public virtual ClientCompany ClientCompany { get; set; }
    }
}
