namespace OnBoarding.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class DeletedEntity
    {
        public int Id { get; set; }
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
        public string DeletedBy { get; set; }
        [Column(TypeName = "datetime2")]
        public DateTime DateDeleted { get; set; } = DateTime.Now;
    }
}
