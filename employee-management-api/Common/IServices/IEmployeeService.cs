using Common.Dto;

namespace Common.IServices
{
    public interface IEmployeeService
    {
        Task<PagedResultDto<EmployeeDto>> GetAllEmployeesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search, int? departmentId, int? positionId, string? status);
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto, string? createdBy);
        Task<bool> UpdateEmployeeAsync(EmployeeDto employeeDto, string? modifiedBy);
        Task<bool> DeleteEmployeeAsync(int id, string? deletedBy);
        Task<EmployeeDto?> UpdateProfileImageAsync(int id, string relativePath);
        Task<bool> RemoveProfileImageAsync(int id);
    }
}