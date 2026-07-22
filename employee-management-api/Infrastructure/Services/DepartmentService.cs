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

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments.Select(ToDto);
        }

        public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            return department == null ? null : ToDto(department);
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
            return ToDto(department);
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

        public async Task<DepartmentDeleteResult> DeleteDepartmentAsync(int id)
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

            await _departmentRepository.DeleteAsync(id);
            return DepartmentDeleteResult.Success;
        }

        private static DepartmentDto ToDto(Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                DepartmentCode = department.DepartmentCode,
                NameEn = department.NameEn,
                NameAr = department.NameAr,
                Description = department.Description,
                Status = department.Status,
                CreatedBy = department.CreatedBy,
                CreationDate = department.CreationDate,
                ModifiedBy = department.ModifiedBy,
                ModificationDate = department.ModificationDate
            };
        }
    }
}