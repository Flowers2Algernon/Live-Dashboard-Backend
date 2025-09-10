// LERD.Domain/Models/NPSModels.cs
namespace LERD.Domain.Models;

public class NPSData
{
    public int NPSScore { get; set; }  // Net Promoter Score (-100到100)
    public NPSDistribution Distribution { get; set; } = new();
}

public class NPSDistribution
{
    public int PromoterCount { get; set; }
    public int PassiveCount { get; set; }
    public int DetractorCount { get; set; }
    public int TotalCount { get; set; }
    
    // 百分比（如果前端需要）
    public decimal PromoterPercentage { get; set; }
    public decimal PassivePercentage { get; set; }
    public decimal DetractorPercentage { get; set; }
}
