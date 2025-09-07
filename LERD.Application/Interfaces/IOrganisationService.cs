using LERD.Shared.DTOs;

namespace LERD.Application.Interfaces
{
    public interface IOrganisationService
    {
        Task<OrganisationDto?> GetByIdAsync(Guid id);
        Task<PagedResult<OrganisationDto>> GetAllAsync(int page = 1, int pageSize = 10);
        Task<PagedResult<OrganisationDto>> GetAllAsync(int page, int pageSize, bool includeCount);
        Task<OrganisationDto> CreateAsync(CreateOrganisationRequest request);
        Task<OrganisationDto> UpdateAsync(Guid id, UpdateOrganisationRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}