using System.ComponentModel.DataAnnotations;

namespace LERD.Shared.DTOs
{
    public class SubscriptionDto
    {
        public Guid Id { get; set; }
        public Guid OrganisationId { get; set; }
        public string? PlanType { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int MaxSurveys { get; set; }
        public int MaxUsers { get; set; }
        public string Features { get; set; } = "{}";
        public string BillingCycle { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Additional computed properties
        public bool IsActive { get; set; }
        public bool IsExpired { get; set; }
        public int? DaysUntilExpiry { get; set; }
        
        // Navigation data
        public string? OrganisationName { get; set; }
    }

    public class CreateSubscriptionRequest
    {
        [Required]
        public Guid OrganisationId { get; set; }
        
        [MaxLength(100)]
        public string? PlanType { get; set; }
        
        [Required]
        [RegularExpression("^(active|inactive|cancelled|expired|suspended)$", 
            ErrorMessage = "Status must be one of: active, inactive, cancelled, expired, suspended")]
        public string Status { get; set; } = "active";
        
        [Required]
        public DateOnly StartDate { get; set; }
        
        public DateOnly? EndDate { get; set; }
        
        [Range(1, 10000)]
        public int MaxSurveys { get; set; } = 100;
        
        [Range(1, 10000)]
        public int MaxUsers { get; set; } = 100;
        
        public string? Features { get; set; }
        
        [RegularExpression("^(monthly|annually)$", 
            ErrorMessage = "Billing cycle must be either 'monthly' or 'annually'")]
        public string BillingCycle { get; set; } = "monthly";
    }

    public class UpdateSubscriptionRequest
    {
        [MaxLength(100)]
        public string? PlanType { get; set; }
        
        [RegularExpression("^(active|inactive|cancelled|expired|suspended)$", 
            ErrorMessage = "Status must be one of: active, inactive, cancelled, expired, suspended")]
        public string? Status { get; set; }
        
        public DateOnly? StartDate { get; set; }
        
        public DateOnly? EndDate { get; set; }
        
        [Range(1, 10000)]
        public int? MaxSurveys { get; set; }
        
        [Range(1, 10000)]
        public int? MaxUsers { get; set; }
        
        public string? Features { get; set; }
        
        [RegularExpression("^(monthly|annually)$", 
            ErrorMessage = "Billing cycle must be either 'monthly' or 'annually'")]
        public string? BillingCycle { get; set; }
    }

    public class OrganisationWithSubscriptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Subscription information
        public SubscriptionDto? Subscription { get; set; }
    }

    public class SubscriptionStatusResponse
    {
        public bool HasActiveSubscription { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateOnly? ExpiryDate { get; set; }
        public int? DaysUntilExpiry { get; set; }
        public int MaxSurveys { get; set; }
        public int MaxUsers { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}