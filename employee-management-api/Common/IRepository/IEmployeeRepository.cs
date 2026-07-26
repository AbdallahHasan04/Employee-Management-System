using Core.Entities;

namespace Common.IRepository
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
        Task<bool> ExistsByDepartmentIdAsync(int departmentId);
        Task<int> GetCountByDepartmentIdAsync(int departmentId);
        Task<Dictionary<int, int>> GetEmployeeCountsByDepartmentAsync();
    }
}