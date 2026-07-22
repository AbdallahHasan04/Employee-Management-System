using Common.Dto;

namespace Common.IServices
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync();
        Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto, string? createdBy);
        Task<bool> UpdateEmployeeAsync(EmployeeDto employeeDto, string? modifiedBy);
        Task<bool> DeleteEmployeeAsync(int id);
    }
}