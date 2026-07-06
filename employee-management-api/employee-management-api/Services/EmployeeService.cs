using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Models;
using EmployeeManagementAPI.Repositories;
namespace EmployeeManagementAPI.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<EmployeeDto> GetAllEmployees() 
        {
            var employees = _repository.GetAll();
            var dtos = new List<EmployeeDto>();

            foreach( var emp in employees)
            {
                dtos.Add(new EmployeeDto()
                {
                    Id = emp.Id,
                    Name = emp.Name,
                    Position = emp.Position,
                    Email = emp.Email,
                });
            }
            return dtos;
        }

        public EmployeeDto? GetEmployeeById(int id) 
        { 
            var emp = _repository.GetById(id);
            if ( emp == null )
            {
                return null;
            }

            return new EmployeeDto
            {
                Id = emp.Id,
                Name = emp.Name,
                Position = emp.Position,
                Email = emp.Email,
            };
        }

        public void CreateEmployee(EmployeeDto employeeDto)
        {
            var model = new Employee
            {
                Name = employeeDto.Name,
                Position = employeeDto.Position,
                Email = employeeDto.Email,
            };

            _repository.Add(model);
        }

        public void UpdateEmployee(EmployeeDto employeeDto)
        {
            var model = new Employee
            {
                Id = employeeDto.Id,
                Name = employeeDto.Name,
                Position = employeeDto.Position,
                Email = employeeDto.Email,
            };

            _repository.Update(model);
        }

        public void DeleteEmployee(int id)
        {
            _repository.Delete(id);
        }

    }
}
