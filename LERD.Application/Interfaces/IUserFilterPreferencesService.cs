using LERD.Shared.DTOs;

namespace LERD.Application.Interfaces;

public interface IUserFilterPreferencesService
{
    // 读取APIs - 获取选项列表
    Task<List<ServiceOption>> GetAvailableServicesAsync(Guid userId);
    Task<List<RegionOption>> GetAvailableRegionsAsync(Guid surveyId);
    Task<FilterOptions> GetFilterOptionsAsync(Guid surveyId);
    
    // 读取APIs - 获取当前选择
    Task<FilterConfiguration> GetUserFiltersAsync(Guid userId, Guid surveyId);
    
    // 写入APIs - 保存用户选择
    Task UpdateServiceSelectionAsync(Guid userId, Guid surveyId);
    Task UpdateRegionSelectionAsync(Guid userId, Guid surveyId, List<string> regions);
    
    // 初始化
    Task<FilterConfiguration> InitializeDefaultFiltersAsync(Guid userId, Guid surveyId);
}
