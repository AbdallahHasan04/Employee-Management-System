using Data.Context;
using Common.IRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _context.Employees.AsNoTracking().Include(e => e.Department).ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _context.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task AddAsync(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsByDepartmentIdAsync(int departmentId)
        {
            return await _context.Employees.AnyAsync(e => e.DepartmentId == departmentId);
        }

        public async Task<(List<Employee> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var query = _context.Employees.AsNoTracking().Include(e => e.Department).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.EmployeeNo.Contains(search) ||
                    e.NameEn.Contains(search) ||
                    e.NameAr.Contains(search) ||
                    e.Username.Contains(search) ||
                    e.NationalNo.Contains(search) ||
                    (e.Email != null && e.Email.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            query = sortBy switch
            {
                "employeeNo" => sortDescending ? query.OrderByDescending(e => e.EmployeeNo) : query.OrderBy(e => e.EmployeeNo),
                "nameEn" => sortDescending ? query.OrderByDescending(e => e.NameEn) : query.OrderBy(e => e.NameEn),
                "nameAr" => sortDescending ? query.OrderByDescending(e => e.NameAr) : query.OrderBy(e => e.NameAr),
                "departmentName" => sortDescending ? query.OrderByDescending(e => e.Department!.NameEn) : query.OrderBy(e => e.Department!.NameEn),
                "username" => sortDescending ? query.OrderByDescending(e => e.Username) : query.OrderBy(e => e.Username),
                "nationalNo" => sortDescending ? query.OrderByDescending(e => e.NationalNo) : query.OrderBy(e => e.NationalNo),
                "gender" => sortDescending ? query.OrderByDescending(e => e.Gender) : query.OrderBy(e => e.Gender),
                "birthdate" => sortDescending ? query.OrderByDescending(e => e.Birthdate) : query.OrderBy(e => e.Birthdate),
                "mobileNumber" => sortDescending ? query.OrderByDescending(e => e.MobileNumber) : query.OrderBy(e => e.MobileNumber),
                "email" => sortDescending ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
                "startWorkingDate" => sortDescending ? query.OrderByDescending(e => e.StartWorkingDate) : query.OrderBy(e => e.StartWorkingDate),
                "status" => sortDescending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                _ => query.OrderBy(e => e.Id)
            };

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }

        public async Task<Dictionary<int, int>> GetEmployeeCountsForDepartmentIdsAsync(IEnumerable<int> departmentIds)
        {
            return await _context.Employees
                .Where(e => departmentIds.Contains(e.DepartmentId))
                .GroupBy(e => e.DepartmentId)
                .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);
        }
    }
}