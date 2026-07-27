using Common.Dto;

namespace Common.IServices
{
    public enum DepartmentDeleteResult
    {
        Success,
        NotFound,
        HasEmployees
    }

    public interface IDepartmentService
    {
        Task<PagedResultDto<DepartmentDto>> GetAllDepartmentsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
        Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto departmentDto, string? createdBy);
        Task<bool> UpdateDepartmentAsync(DepartmentDto departmentDto, string? modifiedBy);
        Task<DepartmentDeleteResult> DeleteDepartmentAsync(int id);
    }
}