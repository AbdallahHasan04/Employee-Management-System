using AutoMapper;
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
        private readonly IMapper _mapper;

        public DepartmentService(IDepartmentRepository departmentRepository, IEmployeeRepository employeeRepository, IMapper mapper)
        {
            _departmentRepository = departmentRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<DepartmentDto>> GetAllDepartmentsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _departmentRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);

            var departmentIds = items.Select(d => d.Id).ToList();
            var counts = await _employeeRepository.GetEmployeeCountsForDepartmentIdsAsync(departmentIds);

            return new PagedResultDto<DepartmentDto>
            {
                Items = items.Select(d => MapToDto(d, counts.GetValueOrDefault(d.Id, 0))).ToList(),
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
            return MapToDto(department, count.GetValueOrDefault(id, 0));
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto dto, string? createdBy)
        {
            var department = _mapper.Map<Department>(dto);
            department.CreatedBy = createdBy;
            department.CreationDate = DateTime.UtcNow;
             
            await _departmentRepository.AddAsync(department);
            return MapToDto(department, 0);
        }

        public async Task<bool> UpdateDepartmentAsync(DepartmentDto dto, string? modifiedBy)
        {
            var existing = await _departmentRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return false;
            }

            _mapper.Map(dto, existing);
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

        private DepartmentDto MapToDto(Department department, int employeeCount)
        {
            var dto = _mapper.Map<DepartmentDto>(department);
            dto.EmployeeCount = employeeCount;
            return dto;
        }
    }
}