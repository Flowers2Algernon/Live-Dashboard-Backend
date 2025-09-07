using System.ComponentModel.DataAnnotations;

namespace LERD.Shared.DTOs
{
    public class OrganisationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? ContactPhone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateOrganisationRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string? ContactPerson { get; set; }
        
        [Phone]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }
    }

    public class UpdateOrganisationRequest
    {
        [MaxLength(255)]
        public string? Name { get; set; }
        
        [MaxLength(255)]
        public string? ContactPerson { get; set; }
        
        [Phone]
        [MaxLength(50)]
        public string? ContactPhone { get; set; }
        
        public bool? IsActive { get; set; }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}