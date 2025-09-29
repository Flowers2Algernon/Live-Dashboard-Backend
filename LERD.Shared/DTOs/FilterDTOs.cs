namespace LERD.Shared.DTOs;

// 过滤器配置主结构
public class FilterConfiguration
{
    public SingleSelectFilter? ServiceType { get; set; }
    public MultiSelectFilter? Region { get; set; }
    public MultiSelectFilter? Gender { get; set; }
    public SingleSelectFilter? ParticipantType { get; set; }
    public DateRangeFilter? Period { get; set; }
}

// 过滤器类型
public class SingleSelectFilter
{
    public string Type { get; set; } = "single_select";
    public string Value { get; set; } = string.Empty;
}

public class MultiSelectFilter
{
    public string Type { get; set; } = "multi_select";
    public List<string> Values { get; set; } = new();
}

public class DateRangeFilter
{
    public string Type { get; set; } = "date_range";
    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;
}

// Service选项
public class ServiceOption
{
    public Guid SurveyId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSelected { get; set; }
    public int TotalResponses { get; set; }
}

// Region选项
public class RegionOption
{
    public string FacilityCode { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public bool IsSelected { get; set; }
}

// 过滤器选项
public class FilterOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public class FilterOptions
{
    public List<FilterOption> Gender { get; set; } = new();
    public List<FilterOption> ParticipantType { get; set; } = new();
    public List<string> Period { get; set; } = new();
}

// Request DTOs
public class UpdateServiceRequest
{
    public Guid SurveyId { get; set; }
    public string ServiceType { get; set; } = string.Empty;
}

public class UpdateRegionsRequest
{
    public Guid SurveyId { get; set; }
    public List<string> Regions { get; set; } = new();
}

// Response DTOs
public class UserFilterResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public FilterConfiguration? Data { get; set; }
}

public class ServicesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ServiceOption>? Data { get; set; }
}

public class RegionsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<RegionOption>? Data { get; set; }
}
