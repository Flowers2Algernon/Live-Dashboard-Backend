using LERD.Application.Interfaces;
using LERD.Domain.Entities;
using LERD.Infrastructure.Data;
using LERD.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LERD.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionDto?> GetByIdAsync(Guid id)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Organisation)
                .FirstOrDefaultAsync(s => s.Id == id);
                
            return subscription == null ? null : MapToDto(subscription);
        }

        public async Task<PagedResult<SubscriptionDto>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            var offset = (page - 1) * pageSize;
            
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Organisation)
                .OrderByDescending(s => s.CreatedAt)
                .Skip(offset)
                .Take(pageSize + 1)
                .ToListAsync();

            var hasMore = subscriptions.Count > pageSize;
            var items = hasMore ? subscriptions.Take(pageSize) : subscriptions;
            
            // 估算总数，避免COUNT查询
            var totalCount = (page == 1 && !hasMore) ? subscriptions.Count : -1;

            return new PagedResult<SubscriptionDto>
            {
                Items = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<SubscriptionDto> CreateAsync(CreateSubscriptionRequest request)
        {
            // 验证组织是否存在
            var organisationExists = await _context.Organisations
                .AnyAsync(o => o.Id == request.OrganisationId && o.IsActive);
                
            if (!organisationExists)
                throw new ArgumentException($"Organisation with ID {request.OrganisationId} not found or inactive");

            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                OrganisationId = request.OrganisationId,
                PlanType = request.PlanType,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                MaxSurveys = request.MaxSurveys,
                MaxUsers = request.MaxUsers,
                Features = request.Features ?? "{}",
                BillingCycle = request.BillingCycle,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(subscription.Id) ?? throw new InvalidOperationException("Failed to retrieve created subscription");
        }

        public async Task<SubscriptionDto> UpdateAsync(Guid id, UpdateSubscriptionRequest request)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (subscription == null)
                throw new ArgumentException($"Subscription with ID {id} not found");

            // Update only provided fields
            if (request.PlanType != null) subscription.PlanType = request.PlanType;
            if (request.Status != null) subscription.Status = request.Status;
            if (request.StartDate.HasValue) subscription.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue) subscription.EndDate = request.EndDate.Value;
            if (request.MaxSurveys.HasValue) subscription.MaxSurveys = request.MaxSurveys.Value;
            if (request.MaxUsers.HasValue) subscription.MaxUsers = request.MaxUsers.Value;
            if (request.Features != null) subscription.Features = request.Features;
            if (request.BillingCycle != null) subscription.BillingCycle = request.BillingCycle;
            
            subscription.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(subscription.Id) ?? throw new InvalidOperationException("Failed to retrieve updated subscription");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (subscription == null) return false;

            // 软删除：设置状态为 cancelled
            subscription.Status = Subscription.StatusTypes.Cancelled;
            subscription.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<SubscriptionDto?> GetActiveSubscriptionByOrganisationAsync(Guid organisationId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.Organisation)
                .Where(s => s.OrganisationId == organisationId)
                .Where(s => s.Status == Subscription.StatusTypes.Active)
                .Where(s => s.EndDate == null || s.EndDate >= DateOnly.FromDateTime(DateTime.UtcNow))
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
                
            return subscription == null ? null : MapToDto(subscription);
        }

        public async Task<IEnumerable<SubscriptionDto>> GetSubscriptionsByOrganisationAsync(Guid organisationId)
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.Organisation)
                .Where(s => s.OrganisationId == organisationId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
                
            return subscriptions.Select(MapToDto);
        }

        public async Task<OrganisationWithSubscriptionDto?> GetOrganisationWithSubscriptionAsync(Guid organisationId)
        {
            var organisation = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Id == organisationId);
                
            if (organisation == null) return null;

            var activeSubscription = await GetActiveSubscriptionByOrganisationAsync(organisationId);
            
            return new OrganisationWithSubscriptionDto
            {
                Id = organisation.Id,
                Name = organisation.Name,
                ContactPerson = organisation.ContactPerson,
                ContactPhone = organisation.ContactPhone,
                IsActive = organisation.IsActive,
                CreatedAt = organisation.CreatedAt,
                UpdatedAt = organisation.UpdatedAt,
                Subscription = activeSubscription
            };
        }

        public async Task<SubscriptionStatusResponse> CheckSubscriptionStatusAsync(Guid organisationId)
        {
            var activeSubscription = await GetActiveSubscriptionByOrganisationAsync(organisationId);
            
            if (activeSubscription == null)
            {
                return new SubscriptionStatusResponse
                {
                    HasActiveSubscription = false,
                    Status = "no_subscription",
                    Message = "No active subscription found for this organisation"
                };
            }

            var daysUntilExpiry = activeSubscription.EndDate.HasValue 
                ? (activeSubscription.EndDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days
                : (int?)null;

            return new SubscriptionStatusResponse
            {
                HasActiveSubscription = activeSubscription.IsActive,
                Status = activeSubscription.Status,
                ExpiryDate = activeSubscription.EndDate,
                DaysUntilExpiry = daysUntilExpiry,
                MaxSurveys = activeSubscription.MaxSurveys,
                MaxUsers = activeSubscription.MaxUsers,
                Message = activeSubscription.IsActive ? "Subscription is active" : 
                         activeSubscription.IsExpired ? "Subscription has expired" : 
                         "Subscription is inactive"
            };
        }

        public async Task<bool> ValidateOrganisationAccessAsync(Guid organisationId)
        {
            var status = await CheckSubscriptionStatusAsync(organisationId);
            return status.HasActiveSubscription;
        }

        public async Task<int> UpdateExpiredSubscriptionsAsync()
        {
            var expiredSubscriptions = await _context.Subscriptions
                .Where(s => s.Status == Subscription.StatusTypes.Active)
                .Where(s => s.EndDate.HasValue && s.EndDate < DateOnly.FromDateTime(DateTime.UtcNow))
                .ToListAsync();

            foreach (var subscription in expiredSubscriptions)
            {
                subscription.Status = Subscription.StatusTypes.Expired;
                subscription.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return expiredSubscriptions.Count;
        }

        public async Task<SubscriptionDto> CreateOrUpdateOrganisationSubscriptionAsync(Guid organisationId, CreateSubscriptionRequest request)
        {
            // 检查是否已有活跃订阅
            var existingSubscription = await _context.Subscriptions
                .Where(s => s.OrganisationId == organisationId)
                .Where(s => s.Status == Subscription.StatusTypes.Active)
                .FirstOrDefaultAsync();

            if (existingSubscription != null)
            {
                // 更新现有订阅
                var updateRequest = new UpdateSubscriptionRequest
                {
                    PlanType = request.PlanType,
                    Status = request.Status,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    MaxSurveys = request.MaxSurveys,
                    MaxUsers = request.MaxUsers,
                    Features = request.Features,
                    BillingCycle = request.BillingCycle
                };
                
                return await UpdateAsync(existingSubscription.Id, updateRequest);
            }
            else
            {
                // 创建新订阅
                request.OrganisationId = organisationId;
                return await CreateAsync(request);
            }
        }

        private static SubscriptionDto MapToDto(Subscription subscription)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var daysUntilExpiry = subscription.EndDate.HasValue 
                ? (subscription.EndDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days
                : (int?)null;

            return new SubscriptionDto
            {
                Id = subscription.Id,
                OrganisationId = subscription.OrganisationId,
                PlanType = subscription.PlanType,
                Status = subscription.Status,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                MaxSurveys = subscription.MaxSurveys,
                MaxUsers = subscription.MaxUsers,
                Features = subscription.Features,
                BillingCycle = subscription.BillingCycle,
                CreatedAt = subscription.CreatedAt,
                UpdatedAt = subscription.UpdatedAt,
                IsActive = subscription.IsActive,
                IsExpired = subscription.IsExpired,
                DaysUntilExpiry = daysUntilExpiry,
                OrganisationName = subscription.Organisation?.Name
            };
        }
    }
}