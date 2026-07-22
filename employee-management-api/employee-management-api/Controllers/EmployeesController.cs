using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Common.Dto;
using Common.IServices;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;
        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> Get()
        {
            return Ok(await _service.GetAllEmployeesAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetById(int id)
        {
            var employee = await _service.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with id {id} not found" });
            }
            return Ok(employee);
        }

        [HttpPost]
        public async Task<ActionResult> Create(EmployeeDto employeeDto)
        {
            var createdBy = User.Identity?.Name;
            var created = await _service.CreateEmployeeAsync(employeeDto, createdBy);
            return Ok(new
            {
                message = "Employee created successfully! A linked user account was created too.",
                employee = created
            });
        }

        [HttpPut]
        public async Task<ActionResult> Update(EmployeeDto employeeDto)
        {
            var modifiedBy = User.Identity?.Name;
            var updated = await _service.UpdateEmployeeAsync(employeeDto, modifiedBy);
            if (!updated)
            {
                return NotFound(new { message = $"Cannot update. Employee with ID {employeeDto.Id} not found." });
            }
            return Ok(new { message = "Employee updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteEmployeeAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Cannot delete. Employee with ID {id} not found." });
            }
            return Ok(new { message = "Employee deleted successfully!" });
        }
    }
}