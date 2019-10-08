namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AspNetRole
    {
        public string Id { get; set; }

        [Required]
        [StringLength(256)]
        public string Name { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public int Status { get; set; }
    }
}
