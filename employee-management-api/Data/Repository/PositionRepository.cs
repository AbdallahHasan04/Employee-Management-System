using Data.Context;
using Common.IRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    public class PositionRepository : IPositionRepository
    {
        private readonly AppDbContext _context;

        public PositionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Position?> GetByIdAsync(int id)
        {
            return await _context.Positions.FindAsync(id);
        }

        public async Task AddAsync(Position position)
        {
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Position position)
        {
            _context.Positions.Update(position);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string? deletedBy)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position != null)
            {
                position.IsDeleted = true;
                position.ModifiedBy = deletedBy;
                position.ModificationDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Position> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var query = _context.Positions.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.NameEn.Contains(search) || p.NameAr.Contains(search));
            }

            var totalCount = await query.CountAsync();

            if (sortBy == "employeeCount")
            {
                var countsQuery = query.Select(p => new
                {
                    Position = p,
                    Count = _context.EmployeePositions.Count(ep => ep.PositionId == p.Id && ep.EndDate == null)
                });

                countsQuery = sortDescending
                    ? countsQuery.OrderByDescending(x => x.Count)
                    : countsQuery.OrderBy(x => x.Count);

                var pagedWithCount = await countsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
                return (pagedWithCount.Select(x => x.Position).ToList(), totalCount);
            }

            query = sortBy switch
            {
                "nameEn" => sortDescending ? query.OrderByDescending(p => p.NameEn) : query.OrderBy(p => p.NameEn),
                "nameAr" => sortDescending ? query.OrderByDescending(p => p.NameAr) : query.OrderBy(p => p.NameAr),
                _ => query.OrderBy(p => p.Id)
            };

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Positions.CountAsync();
        }
    }
}