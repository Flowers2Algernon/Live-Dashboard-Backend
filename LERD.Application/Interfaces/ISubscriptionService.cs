using LERD.Shared.DTOs;

namespace LERD.Application.Interfaces
{
    public interface ISubscriptionService
    {
        // Basic CRUD operations
        Task<SubscriptionDto?> GetByIdAsync(Guid id);
        Task<PagedResult<SubscriptionDto>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request);
        Task<SubscriptionDto> UpdateAsync(Guid id, UpdateSubscriptionRequest request);
        Task<bool> DeleteAsync(Guid id);
        
        // Organisation-specific subscription operations
        Task<SubscriptionDto?> GetActiveSubscriptionByOrganisationAsync(Guid organisationId);
        Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByOrganisationAsync(Guid organisationId);
        Task<OrganisationWithSubscriptionDto?> GetOrganisationWithSubscriptionAsync(Guid organisationId);
        
        // Subscription status and validation
        Task<SubscriptionStatusResponse> CheckSubscriptionStatusAsync(Guid organisationId);
        Task<bool> ValidateOrganisationAccessAsync(Guid organisationId);
        Task<int> UpdateExpiredSubscriptionsAsync();
        
        // Admin operations for creating/updating organisation subscriptions
        Task<SubscriptionDto> CreateOrUpdateOrganisationSubscriptionAsync(Guid organisationId, CreateSubscriptionRequest request);
    }
}