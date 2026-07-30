using Core.Entities;

namespace Common.IRepository
{
    public interface IPositionRepository
    {
        Task<Position?> GetByIdAsync(int id);
        Task AddAsync(Position position);
        Task UpdateAsync(Position position);
        Task DeleteAsync(int id, string? deletedBy);
        Task<(List<Position> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
    }
}