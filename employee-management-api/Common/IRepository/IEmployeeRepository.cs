using Core.Entities;

namespace Common.IRepository
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int id, string? deletedBy);
        Task<bool> ExistsByDepartmentIdAsync(int departmentId);
        Task<(List<Employee> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
        Task<Dictionary<int, int>> GetEmployeeCountsForDepartmentIdsAsync(IEnumerable<int> departmentIds);
        Task<(int ActiveCount, int MaleCount, int FemaleCount)> GetEmployeeStatsAsync();
    }
}