using Data.Context;
using Common.IRepository;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Repository
{
    public class EmployeeDocumentRepository : IEmployeeDocumentRepository
    {
        private readonly AppDbContext _context;

        public EmployeeDocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EmployeeDocument?> GetByIdAsync(int id)
        {
            return await _context.EmployeeDocuments
                .IgnoreQueryFilters()
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(EmployeeDocument document)
        {
            _context.EmployeeDocuments.Add(document);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<EmployeeDocument> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var query = _context.EmployeeDocuments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(d => d.Employee)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.DocumentName.Contains(search) ||
                    (d.Employee != null && d.Employee.NameEn.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            query = sortBy switch
            {
                "documentName" => sortDescending ? query.OrderByDescending(d => d.DocumentName) : query.OrderBy(d => d.DocumentName),
                "employeeName" => sortDescending ? query.OrderByDescending(d => d.Employee!.NameEn) : query.OrderBy(d => d.Employee!.NameEn),
                "issueDate" => sortDescending ? query.OrderByDescending(d => d.IssueDate) : query.OrderBy(d => d.IssueDate),
                "expiryDate" => sortDescending ? query.OrderByDescending(d => d.ExpiryDate) : query.OrderBy(d => d.ExpiryDate),
                _ => query.OrderByDescending(d => d.CreationDate)
            };

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return (items, totalCount);
        }
    }
}