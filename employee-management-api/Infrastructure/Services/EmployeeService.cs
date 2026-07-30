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
        private readonly IEmployeePositionRepository _employeePositionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            IEmployeePositionRepository employeePositionRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _employeePositionRepository = employeePositionRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<PagedResultDto<EmployeeDto>> GetAllEmployeesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _employeeRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);

            var employeeIds = items.Select(e => e.Id).ToList();
            var currentPositions = await _employeePositionRepository.GetCurrentPositionsForEmployeeIdsAsync(employeeIds);

            return new PagedResultDto<EmployeeDto>
            {
                Items = items.Select(e => ToDto(e, currentPositions.GetValueOrDefault(e.Id))).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                return null;
            }

            var currentPositions = await _employeePositionRepository.GetCurrentPositionsForEmployeeIdsAsync(new[] { id });
            return ToDto(employee, currentPositions.GetValueOrDefault(id));
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
                await _employeeRepository.AddAsync(employee); // populates employee.Id

                var generatedPassword = PasswordHasher.GenerateFromEmployeeId(employee.Id);
                user.Password = PasswordHasher.Hash(generatedPassword);
                await _userRepository.UpdateAsync(user);

                // Position is mandatory at creation (enforced by the frontend)
                EmployeePosition? initialPosition = null;
                if (dto.PositionId.HasValue)
                {
                    initialPosition = new EmployeePosition
                    {
                        EmployeeId = employee.Id,
                        PositionId = dto.PositionId.Value,
                        StartDate = dto.StartWorkingDate ?? DateTime.UtcNow.Date,
                        EndDate = null,
                        CreatedBy = createdBy,
                        CreationDate = DateTime.UtcNow
                    };
                    await _employeePositionRepository.AddAsync(initialPosition);
                }

                await _unitOfWork.CommitTransactionAsync();

                var result = ToDto(employee, initialPosition);
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

            await _unitOfWork.BeginTransactionAsync();
            try
            {
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

                // Position change detection: only touch EmployeePositions if the
                // submitted PositionId actually differs from what's currently open.
                if (dto.PositionId.HasValue)
                {
                    var currentPosition = await _employeePositionRepository.GetCurrentByEmployeeIdAsync(dto.Id);

                    if (currentPosition == null || currentPosition.PositionId != dto.PositionId.Value)
                    {
                        var changeDate = DateTime.UtcNow.Date;

                        if (currentPosition != null)
                        {
                            currentPosition.EndDate = changeDate;
                            currentPosition.ModifiedBy = modifiedBy;
                            currentPosition.ModificationDate = DateTime.UtcNow;
                            await _employeePositionRepository.UpdateAsync(currentPosition);
                        }

                        var newPosition = new EmployeePosition
                        {
                            EmployeeId = dto.Id,
                            PositionId = dto.PositionId.Value,
                            StartDate = changeDate,
                            EndDate = null,
                            CreatedBy = modifiedBy,
                            CreationDate = DateTime.UtcNow
                        };
                        await _employeePositionRepository.AddAsync(newPosition);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> DeleteEmployeeAsync(int id, string? deletedBy)
        {
            var existing = await _employeeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return false;
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Close out any currently-open position record but the historical record itself is preserved, not deleted.
                var currentPosition = await _employeePositionRepository.GetCurrentByEmployeeIdAsync(id);
                if (currentPosition != null)
                {
                    currentPosition.EndDate = DateTime.UtcNow.Date;
                    currentPosition.ModifiedBy = deletedBy;
                    currentPosition.ModificationDate = DateTime.UtcNow;
                    await _employeePositionRepository.UpdateAsync(currentPosition);
                }

                await _employeeRepository.DeleteAsync(id, deletedBy);
                await _userRepository.DeleteByUsernameAsync(existing.Username, deletedBy);

                await _unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<EmployeeDto?> UpdateProfileImageAsync(int id, string relativePath)
        {
            var existing = await _employeeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return null;
            }

            _fileStorageService.DeleteEmployeePhoto(existing.ProfileImagePath);

            existing.ProfileImagePath = relativePath;
            await _employeeRepository.UpdateAsync(existing);

            var currentPositions = await _employeePositionRepository.GetCurrentPositionsForEmployeeIdsAsync(new[] { id });
            return ToDto(existing, currentPositions.GetValueOrDefault(id));
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

        private static EmployeeDto ToDto(Employee employee, EmployeePosition? currentPosition = null)
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
                PositionId = currentPosition?.PositionId,
                PositionName = currentPosition?.Position?.NameEn,
                ProfileImagePath = employee.ProfileImagePath,
                CreatedBy = employee.CreatedBy,
                CreationDate = employee.CreationDate,
                ModifiedBy = employee.ModifiedBy,
                ModificationDate = employee.ModificationDate
            };
        }
    }
}