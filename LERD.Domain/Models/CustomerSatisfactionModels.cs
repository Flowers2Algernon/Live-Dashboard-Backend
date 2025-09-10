// LERD.Domain/Models/CustomerSatisfactionModels.cs
namespace LERD.Domain.Models;

public class CustomerSatisfactionData
{
    public decimal VerySatisfiedPercentage { get; set; }
    public decimal SatisfiedPercentage { get; set; }
    public decimal SomewhatSatisfiedPercentage { get; set; }
    public decimal TotalSatisfiedPercentage { get; set; }
}
