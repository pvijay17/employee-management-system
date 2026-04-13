namespace EMS.API.DTOs.Employees;

public class EmployeeQueryParametersDto
{
    public string? Search { get; set; }
    public string? Department { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDir { get; set; } = "desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
