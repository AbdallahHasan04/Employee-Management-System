using Common.Dto;

namespace Common.IServices
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
        Task<List<DepartmentEmployeeCountDto>> GetEmployeesByDepartmentAsync();
    }
}