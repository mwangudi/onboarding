namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Notification
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(50)]
        public string To { get; set; }

        [Required]
        [StringLength(50)]
        public string From { get; set; }

        [Required]
        public string MessageBody { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool Sent { get; set; }

        public int Attempts { get; set; }
        public string Action { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime LastDateResent { get; set; }

    }
}
