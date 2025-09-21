// LERD.Application/Interfaces/INPSService.cs
using LERD.Domain.Models;

namespace LERD.Application.Interfaces;

public interface INPSService
{
    Task<NPSData> GetNPSDataAsync(Guid surveyId, ChartFilters filters);
}
