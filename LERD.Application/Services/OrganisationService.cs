using LERD.Application.Interfaces;
using LERD.Domain.Entities;
using LERD.Infrastructure.Data;
using LERD.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LERD.Application.Services
{
    public class OrganisationService : IOrganisationService
    {
        private readonly ApplicationDbContext _context;

        public OrganisationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrganisationDto?> GetByIdAsync(Guid id)
        {
            var org = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Id == id);
                
            return org == null ? null : MapToDto(org);
        }

        public async Task<PagedResult<OrganisationDto>> GetAllAsync(int page = 1, int pageSize = 10, bool includeCount = false)
        {
            var offset = (page - 1) * pageSize;
            
            // 首先尝试获取数据，避免COUNT操作导致超时
            var organisations = await _context.Organisations
                .OrderBy(o => o.Name)
                .Skip(offset)
                .Take(pageSize + 1) // 多取一个来判断是否还有更多数据
                .ToListAsync();

            var hasMore = organisations.Count > pageSize;
            var items = hasMore ? organisations.Take(pageSize) : organisations;
            
            int totalCount;
            if (includeCount)
            {
                try
                {
                    // 只有在明确要求时才获取总数，并设置较短的超时
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    totalCount = await _context.Organisations.CountAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // 如果COUNT超时，返回-1表示未知
                    totalCount = -1;
                }
            }
            else
            {
                // 为了向后兼容，我们提供一个估计的总数
                // 如果请求的是第一页且数据少于pageSize，那么总数就是数据量
                // 否则我们返回-1表示总数未知
                totalCount = (page == 1 && !hasMore) ? organisations.Count : -1;
            }

            return new PagedResult<OrganisationDto>
            {
                Items = items.Select(MapToDto),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // 保留原有的方法签名以确保向后兼容
        public async Task<PagedResult<OrganisationDto>> GetAllAsync(int page = 1, int pageSize = 10)
        {
            return await GetAllAsync(page, pageSize, false);
        }

        public async Task<OrganisationDto> CreateAsync(CreateOrganisationRequest request)
        {
            var organisation = new Organisation
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                ContactPerson = request.ContactPerson,
                ContactPhone = request.ContactPhone,
                Settings = "{}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Organisations.Add(organisation);
            await _context.SaveChangesAsync();

            return MapToDto(organisation);
        }

        public async Task<OrganisationDto> UpdateAsync(Guid id, UpdateOrganisationRequest request)
        {
            var organisation = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (organisation == null)
                throw new ArgumentException($"Organisation with ID {id} not found");

            if (request.Name != null) organisation.Name = request.Name;
            if (request.ContactPerson != null) organisation.ContactPerson = request.ContactPerson;
            if (request.ContactPhone != null) organisation.ContactPhone = request.ContactPhone;
            if (request.IsActive.HasValue) organisation.IsActive = request.IsActive.Value;
            
            organisation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(organisation);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var organisation = await _context.Organisations
                .FirstOrDefaultAsync(o => o.Id == id);
                
            if (organisation == null) return false;

            // 软删除
            organisation.IsActive = false;
            organisation.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }

        private static OrganisationDto MapToDto(Organisation org)
        {
            return new OrganisationDto
            {
                Id = org.Id,
                Name = org.Name,
                ContactPerson = org.ContactPerson,
                ContactPhone = org.ContactPhone,
                IsActive = org.IsActive,
                CreatedAt = org.CreatedAt,
                UpdatedAt = org.UpdatedAt
            };
        }
    }
}