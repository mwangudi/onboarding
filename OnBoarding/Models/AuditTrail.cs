namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class AuditTrail
    {
        public int Id { get; set; }
        public string ActionType { get; set; }
        public int EntityId { get; set; }
        public string EntityUId { get; set; }
        [StringLength(256)]
        public string EntityTable { get; set; }
        [StringLength(256)]
        public string EntityName { get; set; }
        [StringLength(256)]
        public string EntityEmail { get; set; }
        [StringLength(256)]
        public string EntityPhone { get; set; }
        public string DoneBy { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime DateCreated { get; set; } = DateTime.Now;
    }
}
