using Core.Entities;

namespace Common.IRepository
{
    public interface IEmployeePositionRepository
    {
        Task<(List<EmployeePosition> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
        Task<bool> ExistsByPositionIdAsync(int positionId);
        Task<EmployeePosition?> GetCurrentByEmployeeIdAsync(int employeeId);
        Task<Dictionary<int, EmployeePosition>> GetCurrentPositionsForEmployeeIdsAsync(IEnumerable<int> employeeIds);
        Task<Dictionary<int, int>> GetCurrentEmployeeCountsForPositionIdsAsync(IEnumerable<int> positionIds);
        Task AddAsync(EmployeePosition employeePosition);
        Task UpdateAsync(EmployeePosition employeePosition);
    }
}