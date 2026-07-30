using Common.Dto;
using Common.IRepository;
using Common.IServices;
using Core.Entities;

namespace Infrastructure.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DepartmentService(IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository)
        {
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<PagedResultDto<DepartmentDto>> GetAllDepartmentsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _departmentRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);

            var departmentIds = items.Select(d => d.Id).ToList();
            var counts = await _employeeRepository.GetEmployeeCountsForDepartmentIdsAsync(departmentIds);

            return new PagedResultDto<DepartmentDto>
            {
                Items = items.Select(d => ToDto(d, counts.GetValueOrDefault(d.Id, 0))).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return null;
            }

            var count = await _employeeRepository.GetEmployeeCountsForDepartmentIdsAsync(new[] { id });
            return ToDto(department, count.GetValueOrDefault(id, 0));
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto dto, string? createdBy)
        {
            var department = new Department
            {
                DepartmentCode = dto.DepartmentCode,
                NameEn = dto.NameEn,
                NameAr = dto.NameAr,
                Description = dto.Description,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status,
                CreatedBy = createdBy,
                CreationDate = DateTime.UtcNow
            };

            await _departmentRepository.AddAsync(department);
            return ToDto(department, 0);
        }

        public async Task<bool> UpdateDepartmentAsync(DepartmentDto dto, string? modifiedBy)
        {
            var existing = await _departmentRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return false;
            }

            existing.DepartmentCode = dto.DepartmentCode;
            existing.NameEn = dto.NameEn;
            existing.NameAr = dto.NameAr;
            existing.Description = dto.Description;
            existing.Status = dto.Status;
            existing.ModifiedBy = modifiedBy;
            existing.ModificationDate = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(existing);
            return true;
        }

        public async Task<DepartmentDeleteResult> DeleteDepartmentAsync(int id, string? deletedBy)
        {
            var existing = await _departmentRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return DepartmentDeleteResult.NotFound;
            }

            var hasEmployees = await _employeeRepository.ExistsByDepartmentIdAsync(id);
            if (hasEmployees)
            {
                return DepartmentDeleteResult.HasEmployees;
            }

            await _departmentRepository.DeleteAsync(id, deletedBy);
            return DepartmentDeleteResult.Success;
        }

        private static DepartmentDto ToDto(Department department, int employeeCount)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                DepartmentCode = department.DepartmentCode,
                NameEn = department.NameEn,
                NameAr = department.NameAr,
                Description = department.Description,
                Status = department.Status,
                EmployeeCount = employeeCount,
                CreatedBy = department.CreatedBy,
                CreationDate = department.CreationDate,
                ModifiedBy = department.ModifiedBy,
                ModificationDate = department.ModificationDate
            };
        }
    }
}