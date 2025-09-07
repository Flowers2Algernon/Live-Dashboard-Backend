using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LERD.Domain.Entities
{
    [Table("subscriptions")]
    public class Subscription
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        
        [Required]
        [Column("organisation_id")]
        public Guid OrganisationId { get; set; }
        
        [Column("plan_type")]
        public string? PlanType { get; set; }
        
        [Required]
        [Column("status")]
        public string Status { get; set; } = "active";
        
        [Required]
        [Column("start_date")]
        public DateOnly StartDate { get; set; }
        
        [Column("end_date")]
        public DateOnly? EndDate { get; set; }
        
        [Column("max_surveys")]
        public int MaxSurveys { get; set; } = 100;
        
        [Column("max_users")]
        public int MaxUsers { get; set; } = 100;
        
        [Column("features", TypeName = "jsonb")]
        public string Features { get; set; } = "{}";
        
        [Column("billing_cycle")]
        public string BillingCycle { get; set; } = "monthly";
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation property
        [ForeignKey("OrganisationId")]
        public virtual Organisation? Organisation { get; set; }
        
        // Helper methods for status checking
        public bool IsActive => Status == "active" && (EndDate == null || EndDate >= DateOnly.FromDateTime(DateTime.UtcNow));
        public bool IsExpired => EndDate.HasValue && EndDate < DateOnly.FromDateTime(DateTime.UtcNow);
        
        // Status constants
        public static class StatusTypes
        {
            public const string Active = "active";
            public const string Inactive = "inactive"; 
            public const string Cancelled = "cancelled";
            public const string Expired = "expired";
            public const string Suspended = "suspended";
        }
        
        // Billing cycle constants
        public static class BillingCycles
        {
            public const string Monthly = "monthly";
            public const string Annually = "annually";
        }
    }
}