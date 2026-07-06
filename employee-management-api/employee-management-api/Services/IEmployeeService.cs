using EmployeeManagementAPI.DTOs;
namespace EmployeeManagementAPI.Services
{
    public interface IEmployeeService
    {
        IEnumerable<EmployeeDto> GetAllEmployees();
        EmployeeDto? GetEmployeeById(int id);
        void CreateEmployee(EmployeeDto employeeDto);
        void UpdateEmployee(EmployeeDto employeeDto);
        void DeleteEmployee(int id);
    }
}