// LERD.Application/Interfaces/ICustomerSatisfactionTrendService.cs
using LERD.Domain.Models;

namespace LERD.Application.Interfaces;

public interface ICustomerSatisfactionTrendService
{
    Task<CustomerSatisfactionTrendData> GetTrendDataAsync(Guid surveyId, ChartFilters filters);
}
