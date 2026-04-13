namespace EMS.API.DTOs.Employees;

public class EmployeeListResponseDto
{
    public IReadOnlyCollection<EmployeeListItemDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
