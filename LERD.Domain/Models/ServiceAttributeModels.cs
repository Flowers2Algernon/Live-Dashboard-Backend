// LERD.Domain/Models/ServiceAttributeModels.cs
namespace LERD.Domain.Models;

public class ServiceAttributeData
{
    public List<AttributeItem> Attributes { get; set; } = new();
    public List<string> AvailableAttributes { get; set; } = new(); // 用于过滤器
}

public class AttributeItem
{
    public string AttributeName { get; set; } = string.Empty;
    public int TotalResponses { get; set; }
    public int ValidResponses { get; set; }
    public int AlwaysCount { get; set; }
    public int MostCount { get; set; }
    public decimal AlwaysPercentage { get; set; }
    public decimal MostPercentage { get; set; }
    public decimal Criteria80Percentage { get; set; } = 80.0m;
    public decimal Criteria60Percentage { get; set; } = 60.0m;
}

public class ServiceAttributeFilters : ChartFilters
{
    public List<string>? SelectedAttributes { get; set; } // 图表级别的属性过滤
}
