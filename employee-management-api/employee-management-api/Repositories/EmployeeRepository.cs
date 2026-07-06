using EmployeeManagementAPI.Models;
namespace EmployeeManagementAPI.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private static readonly List<Employee> _employees = new()
        {
            new Employee { Id = 1, Name = "Abdallah", Position = "Software Engineer", Email = "abdallah@example.com" },
            new Employee { Id = 2, Name = "Ali", Position = "Product Manager", Email = "ali@example.com" }
        };

        private static int _nextId = 3;

        public IEnumerable<Employee> GetAll()
        {
            return _employees;
        }

        public Employee? GetById(int id)
        {
            return _employees.Find(e => e.Id == id);
        }

        public void Add(Employee employee)
        {
            employee.Id = _nextId++;
            _employees.Add(employee);
        }

        public void Update(Employee employee)
        {
            var index = _employees.FindIndex(e => e.Id == employee.Id);
            if (index != -1)
            {
                _employees[index] = employee;
            }
        }

        public void Delete(int id)
        {
            var employee = GetById(id);
            if (employee != null)
            {
                _employees.Remove(employee);
            }
        }
    }
}