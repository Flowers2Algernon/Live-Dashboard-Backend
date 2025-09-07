using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LERD.Domain.Entities
{
    [Table("organisations")]
    public class Organisation
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        
        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;
        
        [Column("contact_person")]
        public string? ContactPerson { get; set; }
        
        [Column("contact_phone")]
        public string? ContactPhone { get; set; }
        
        [Column("settings", TypeName = "jsonb")]
        public string Settings { get; set; } = "{}";
        
        [Column("is_active")]
        public bool IsActive { get; set; } = true;
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}