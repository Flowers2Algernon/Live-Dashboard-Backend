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

        [Column("qualtrics_survey_id")]
        public string? QualtricsId { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("service_type")]
        public string? ServiceType { get; set; }

        [Column("status")]
        public string Status { get; set; } = "active";

        [Column("description")]
        public string? Description { get; set; }

        // 这些字段在实体中保留，但在EF配置中忽略，因为数据库表结构不同
        public string Settings { get; set; } = "{}";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Organisation? Organisation { get; set; }
    }
}
