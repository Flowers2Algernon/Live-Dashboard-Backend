// LERD.Application/Interfaces/IResponseChartService.cs
using LERD.Domain.Models;

namespace LERD.Application.Interfaces;

public interface IResponseChartService
{
    Task<ResponseChartData> GetResponseChartDataAsync(Guid surveyId, ChartFilters filters);
}