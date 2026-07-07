using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Services;
namespace EmployeeManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<EmployeeDto>> Get()
        {
            var employees = _service.GetAllEmployees();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public ActionResult<EmployeeDto> GetById(int id)
        {
            var employee = _service.GetEmployeeById(id);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with id {id} not found" });
            }
            return Ok(employee);
        }

        [HttpPost]
        public ActionResult Create(EmployeeDto employeeDto)
        {
            _service.CreateEmployee(employeeDto);
            return Ok(new { message = "Employee created successfully!" });
        }

        [HttpPut]
        public ActionResult Update(EmployeeDto employeeDto)
        {
            var existing = _service.UpdateEmployee(employeeDto);
            if (!existing)
            {
                return NotFound(new { message = $"Cannot update. Employee with ID {employeeDto.Id} not found." });
            }
            return Ok(new { message = "Employee updated successfully!" });
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _service.DeleteEmployee(id);
            if (!existing)
            {
                return NotFound(new { message = $"Cannot delete. Employee with ID {id} not found." });
            }
            return Ok(new { message = "Employee deleted successfully!" });
        }
    }
}
