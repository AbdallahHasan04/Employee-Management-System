using AutoMapper;
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
        private readonly IMapper _mapper;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            IUserRepository userRepository,
            IEmployeePositionRepository employeePositionRepository,
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _employeePositionRepository = employeePositionRepository;
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<EmployeeDto>> GetAllEmployeesAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search, int? departmentId, int? positionId, string? status)
        {
            var (items, totalCount) = await _employeeRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search, departmentId, positionId, status);

            var employeeIds = items.Select(e => e.Id).ToList();
            var currentPositions = await _employeePositionRepository.GetCurrentPositionsForEmployeeIdsAsync(employeeIds);

            return new PagedResultDto<EmployeeDto>
            {
                Items = items.Select(e => MapToDto(e, currentPositions.GetValueOrDefault(e.Id))).ToList(),
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
            return MapToDto(employee, currentPositions.GetValueOrDefault(id));
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto, string? createdBy)
        { 
            if (await _userRepository.ExistsByUsernameAsync(dto.Username))
            {
                throw new InvalidOperationException($"Username '{dto.Username}' is already taken.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = _mapper.Map<User>(dto);
                user.Password = PasswordHasher.Hash(Guid.NewGuid().ToString("N")); // temp password until employee.Id exists
                user.CreatedBy = createdBy;
                user.CreationDate = DateTime.UtcNow;
                await _userRepository.AddAsync(user);

                var employee = _mapper.Map<Employee>(dto);
                employee.Username = dto.Username; // ignored by the profile to protect immutability because its FK
                employee.CreatedBy = createdBy;
                employee.CreationDate = DateTime.UtcNow;
                await _employeeRepository.AddAsync(employee); // populates employee.Id

                var generatedPassword = PasswordHasher.GenerateFromEmployeeId(employee.Id);
                user.Password = PasswordHasher.Hash(generatedPassword);
                await _userRepository.UpdateAsync(user);

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

                var result = MapToDto(employee, initialPosition);
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
                _mapper.Map(dto, existing);
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

                        var newPosition = _mapper.Map<EmployeePosition>(dto);
                        newPosition.StartDate = changeDate;
                        newPosition.CreatedBy = modifiedBy;
                        newPosition.CreationDate = DateTime.UtcNow;
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
            return MapToDto(existing, currentPositions.GetValueOrDefault(id));
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

        private EmployeeDto MapToDto(Employee employee, EmployeePosition? currentPosition = null)
        {
            var dto = _mapper.Map<EmployeeDto>(employee);
            dto.PositionId = currentPosition?.PositionId;
            dto.PositionName = currentPosition?.Position?.NameEn;
            return dto;
        }
    }
}