using Data.Context;
using Common.IRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    public class EmployeePositionRepository : IEmployeePositionRepository
    {
        private readonly AppDbContext _context;

        public EmployeePositionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<EmployeePosition> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            // IgnoreQueryFilters + manual own-filter: history must stay visible 
            var query = _context.EmployeePositions
                .IgnoreQueryFilters()
                .Where(ep => !ep.IsDeleted)
                .AsNoTracking()
                .Include(ep => ep.Employee)
                .Include(ep => ep.Position)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(ep =>
                    ep.Employee!.EmployeeNo.Contains(search) ||
                    ep.Employee!.NameEn.Contains(search) ||
                    ep.Employee!.NameAr.Contains(search) ||
                    ep.Position!.NameEn.Contains(search) ||
                    ep.Position!.NameAr.Contains(search));
            }

            var totalCount = await query.CountAsync();

            query = sortBy switch
            {
                "employeeName" => sortDescending ? query.OrderByDescending(ep => ep.Employee!.NameEn) : query.OrderBy(ep => ep.Employee!.NameEn),
                "positionName" => sortDescending ? query.OrderByDescending(ep => ep.Position!.NameEn) : query.OrderBy(ep => ep.Position!.NameEn),
                "startDate" => sortDescending ? query.OrderByDescending(ep => ep.StartDate) : query.OrderBy(ep => ep.StartDate),
                "endDate" => sortDescending ? query.OrderByDescending(ep => ep.EndDate) : query.OrderBy(ep => ep.EndDate),
                _ => query.OrderByDescending(ep => ep.StartDate)
            };

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<bool> ExistsByPositionIdAsync(int positionId)
        {
            return await _context.EmployeePositions.AnyAsync(ep => ep.PositionId == positionId);
        }

        public async Task<EmployeePosition?> GetCurrentByEmployeeIdAsync(int employeeId)
        {
            return await _context.EmployeePositions
                .FirstOrDefaultAsync(ep => ep.EmployeeId == employeeId && ep.EndDate == null);
        }

        public async Task<Dictionary<int, EmployeePosition>> GetCurrentPositionsForEmployeeIdsAsync(IEnumerable<int> employeeIds)
        {
            return await _context.EmployeePositions
                .IgnoreQueryFilters()
                .Where(ep => !ep.IsDeleted)
                .Include(ep => ep.Position)
                .Where(ep => employeeIds.Contains(ep.EmployeeId) && ep.EndDate == null)
                .ToDictionaryAsync(ep => ep.EmployeeId);
        }

        public async Task AddAsync(EmployeePosition employeePosition)
        {
            _context.EmployeePositions.Add(employeePosition);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmployeePosition employeePosition)
        {
            _context.EmployeePositions.Update(employeePosition);
            await _context.SaveChangesAsync();
        }
    }
}