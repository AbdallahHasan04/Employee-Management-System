using Core.Entities;

namespace Common.IRepository
{
    public interface IEmployeeDocumentRepository
    {
        Task<EmployeeDocument?> GetByIdAsync(int id);
        Task AddAsync(EmployeeDocument document);
        Task<(List<EmployeeDocument> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
    }
}