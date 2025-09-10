// LERD.Domain/Models/CustomerSatisfactionTrendModels.cs
namespace LERD.Domain.Models;

public class CustomerSatisfactionTrendData
{
    public List<YearlyTrendData> Years { get; set; } = new();
}

public class YearlyTrendData
{
    public int Year { get; set; }
    public decimal VerySatisfiedPercentage { get; set; }
    public decimal SatisfiedPercentage { get; set; }
    public decimal SomewhatSatisfiedPercentage { get; set; }
    public decimal TotalSatisfiedPercentage { get; set; }
}
