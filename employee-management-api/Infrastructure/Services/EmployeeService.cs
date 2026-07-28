using Common.Dto;
using Common.IRepository;
using Common.IServices;
using Core.Entities;
using Infrastructure.Helpers;

namespace Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<PagedResultDto<EmployeeDto>> GetAllEmployeesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _employeeRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return new PagedResultDto<EmployeeDto>
            {
                Items = items.Select(ToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            return employee == null ? null : ToDto(employee);
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto, string? createdBy)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Username = dto.Username,
                    Name = dto.NameEn,
                    Password = PasswordHasher.Hash(Guid.NewGuid().ToString("N")),
                    Status = "Active",
                    CreatedBy = createdBy,
                    CreationDate = DateTime.UtcNow
                };
                await _userRepository.AddAsync(user);

                var employee = new Employee
                {
                    EmployeeNo = dto.EmployeeNo,
                    NameEn = dto.NameEn,
                    NameAr = dto.NameAr,
                    Username = dto.Username,
                    Birthdate = dto.Birthdate,
                    NationalNo = dto.NationalNo,
                    Gender = dto.Gender,
                    Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status,
                    MobileNumber = dto.MobileNumber,
                    Email = dto.Email,
                    StartWorkingDate = dto.StartWorkingDate,
                    DepartmentId = dto.DepartmentId,
                    CreatedBy = createdBy,
                    CreationDate = DateTime.UtcNow
                };
                await _employeeRepository.AddAsync(employee);

                var generatedPassword = PasswordHasher.GenerateFromEmployeeId(employee.Id);
                user.Password = PasswordHasher.Hash(generatedPassword);
                await _userRepository.UpdateAsync(user);

                await _unitOfWork.CommitTransactionAsync();

                var result = ToDto(employee);
                result.GeneratedPassword = generatedPassword;
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UpdateEmployeeAsync(EmployeeDto dto, string? modifiedBy)
        {
            var existing = await _employeeRepository.GetByIdAsync(dto.Id);
            if (existing == null)
            {
                return false;
            }

            existing.EmployeeNo = dto.EmployeeNo;
            existing.NameEn = dto.NameEn;
            existing.NameAr = dto.NameAr;
            existing.Birthdate = dto.Birthdate;
            existing.NationalNo = dto.NationalNo;
            existing.Gender = dto.Gender;
            existing.Status = dto.Status;
            existing.MobileNumber = dto.MobileNumber;
            existing.Email = dto.Email;
            existing.StartWorkingDate = dto.StartWorkingDate;
            existing.DepartmentId = dto.DepartmentId;
            existing.ModifiedBy = modifiedBy;
            existing.ModificationDate = DateTime.UtcNow;

            await _employeeRepository.UpdateAsync(existing);

            var user = await _userRepository.GetByUsernameAsync(existing.Username);
            if (user != null && user.Name != existing.NameEn)
            {
                user.Name = existing.NameEn;
                user.ModifiedBy = modifiedBy;
                user.ModificationDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }

            return true;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            var existing = await _employeeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            _fileStorageService.DeleteEmployeePhoto(existing.ProfileImagePath);

            await _employeeRepository.DeleteAsync(id);
            await _userRepository.DeleteByUsernameAsync(existing.Username);
            return true;
        }

        public async Task<EmployeeDto?> UpdateProfileImageAsync(int id, string relativePath)
        {
            var existing = await _employeeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            // clean up the old photo file, if one existed, so uploads don't pile up orphaned files
            _fileStorageService.DeleteEmployeePhoto(existing.ProfileImagePath);

            existing.ProfileImagePath = relativePath;
            await _employeeRepository.UpdateAsync(existing);

            return ToDto(existing);
        }

        public async Task<bool> RemoveProfileImageAsync(int id)
        {
            var existing = await _employeeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            _fileStorageService.DeleteEmployeePhoto(existing.ProfileImagePath);
            existing.ProfileImagePath = null;
            await _employeeRepository.UpdateAsync(existing);
            return true;
        }

        private static EmployeeDto ToDto(Employee employee)
        {
            return new EmployeeDto
            {
                Id = employee.Id,
                EmployeeNo = employee.EmployeeNo,
                NameEn = employee.NameEn,
                NameAr = employee.NameAr,
                Username = employee.Username,
                Birthdate = employee.Birthdate,
                NationalNo = employee.NationalNo,
                Gender = employee.Gender,
                Status = employee.Status,
                MobileNumber = employee.MobileNumber,
                Email = employee.Email,
                StartWorkingDate = employee.StartWorkingDate,
                DepartmentId = employee.DepartmentId,
                DepartmentName = employee.Department?.NameEn,
                ProfileImagePath = employee.ProfileImagePath,
                CreatedBy = employee.CreatedBy,
                CreationDate = employee.CreationDate,
                ModifiedBy = employee.ModifiedBy,
                ModificationDate = employee.ModificationDate
            };
        }
    }
}