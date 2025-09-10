// LERD.Application/Interfaces/IServiceAttributeService.cs
using LERD.Domain.Models;

namespace LERD.Application.Interfaces;

public interface IServiceAttributeService
{
    Task<ServiceAttributeData> GetServiceAttributeDataAsync(Guid surveyId, ServiceAttributeFilters filters);
}
