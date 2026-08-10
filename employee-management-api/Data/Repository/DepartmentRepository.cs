using Data.Context;
using Common.IRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly AppDbContext _context;

        public DepartmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments.AsNoTracking().ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task AddAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string? deletedBy)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                department.IsDeleted = true;
                department.ModifiedBy = deletedBy;
                department.ModificationDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<(List<Department> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var query = _context.Departments.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.DepartmentCode.Contains(search) ||
                    d.NameEn.Contains(search) ||
                    d.NameAr.Contains(search) ||
                    (d.Description != null && d.Description.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            if (sortBy == "employeeCount")
            {
                var countsQuery = query.Select(d => new
                {
                    Department = d,
                    Count = _context.Employees.Count(e => e.DepartmentId == d.Id)
                });

                countsQuery = sortDescending
                    ? countsQuery.OrderByDescending(x => x.Count)
                    : countsQuery.OrderBy(x => x.Count);

                var pagedWithCount = await countsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
                return (pagedWithCount.Select(x => x.Department).ToList(), totalCount);
            }

            query = sortBy switch
            {
                "departmentCode" => sortDescending ? query.OrderByDescending(d => d.DepartmentCode) : query.OrderBy(d => d.DepartmentCode),
                "nameEn" => sortDescending ? query.OrderByDescending(d => d.NameEn) : query.OrderBy(d => d.NameEn),
                "nameAr" => sortDescending ? query.OrderByDescending(d => d.NameAr) : query.OrderBy(d => d.NameAr),
                "description" => sortDescending ? query.OrderByDescending(d => d.Description) : query.OrderBy(d => d.Description),
                "status" => sortDescending ? query.OrderByDescending(d => d.Status) : query.OrderBy(d => d.Status),
                _ => query.OrderBy(d => d.Id)
            };

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.Departments.CountAsync();
        }
    }
}