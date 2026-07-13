using EmployeeManagementAPI.Data;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Helpers;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Repositories;

namespace EmployeeManagementAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context; // only used for the cross-table transaction below

        public EmployeeService(IEmployeeRepository employeeRepository, IUserRepository userRepository, AppDbContext context)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<IEnumerable<EmployeeDto>> GetAllEmployeesAsync()
        {
            var employees = await _employeeRepository.GetAllAsync();
            return employees.Select(ToDto);
        }

        public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetByIdAsync(id);
            return employee == null ? null : ToDto(employee);
        }

        public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto dto, string? createdBy)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // create the User first (FK target) with a throwaway placeholder password 
                // it gets overwritten once we know the Employee's real ID.
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

                // create the Employee, referencing that Username
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
                    CreatedBy = createdBy,
                    CreationDate = DateTime.UtcNow
                };
                await _employeeRepository.AddAsync(employee); // populates employee.Id

                // generate the real password
                var generatedPassword = PasswordHasher.GenerateFromEmployeeId(employee.Id);
                user.Password = PasswordHasher.Hash(generatedPassword);
                await _userRepository.UpdateAsync(user);

                await transaction.CommitAsync();

                var result = ToDto(employee);
                result.GeneratedPassword = generatedPassword;
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
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

            // Username is immutable (it's an FK to Users.Username) 
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
            existing.ModifiedBy = modifiedBy;
            existing.ModificationDate = DateTime.UtcNow;

            await _employeeRepository.UpdateAsync(existing);

            // (Username itself never changes)
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

            // Child (Employee) must go before parent (User) — matches the FK direction
            await _employeeRepository.DeleteAsync(id);
            await _userRepository.DeleteByUsernameAsync(existing.Username);
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
                CreatedBy = employee.CreatedBy,
                CreationDate = employee.CreationDate,
                ModifiedBy = employee.ModifiedBy,
                ModificationDate = employee.ModificationDate
            };
        }
    }
}