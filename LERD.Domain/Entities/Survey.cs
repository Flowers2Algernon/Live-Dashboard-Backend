using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LERD.Domain.Entities
{
    [Table("surveys")]
    public class Survey
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("organisation_id")]
        public Guid OrganisationId { get; set; }

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("service_type")]
        public string? ServiceType { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("description")]
        public string? Description { get; set; }

        [Column("settings", TypeName = "jsonb")]
        public string Settings { get; set; } = "{}";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Organisation? Organisation { get; set; }
    }
}
