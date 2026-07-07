using EmployeeManagementAPI.DTOs;
namespace EmployeeManagementAPI.Services
{
    public interface IEmployeeService
    {
        IEnumerable<EmployeeDto> GetAllEmployees();
        EmployeeDto? GetEmployeeById(int id);
        void CreateEmployee(EmployeeDto employeeDto);
        bool UpdateEmployee(EmployeeDto employeeDto);
        bool DeleteEmployee(int id);
    }
}