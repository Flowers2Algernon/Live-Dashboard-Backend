// LERD.Application/Interfaces/ICustomerSatisfactionService.cs
using LERD.Domain.Models;

namespace LERD.Application.Interfaces;

public interface ICustomerSatisfactionService
{
    Task<CustomerSatisfactionData> GetSatisfactionDataAsync(Guid surveyId, ChartFilters filters);
}
