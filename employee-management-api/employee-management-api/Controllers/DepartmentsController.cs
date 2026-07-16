using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementAPI.DTOs;
using EmployeeManagementAPI.Services;

namespace EmployeeManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _service;
        public DepartmentsController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> Get()
        {
            return Ok(await _service.GetAllDepartmentsAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetById(int id)
        {
            var department = await _service.GetDepartmentByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = $"Department with id {id} not found" });
            }
            return Ok(department);
        }

        [HttpPost]
        public async Task<ActionResult> Create(DepartmentDto departmentDto)
        {
            var createdBy = User.Identity?.Name;
            var created = await _service.CreateDepartmentAsync(departmentDto, createdBy);
            return Ok(new
            {
                message = "Department created successfully!",
                department = created
            });
        }

        [HttpPut]
        public async Task<ActionResult> Update(DepartmentDto departmentDto)
        {
            var modifiedBy = User.Identity?.Name;
            var updated = await _service.UpdateDepartmentAsync(departmentDto, modifiedBy);
            if (!updated)
            {
                return NotFound(new { message = $"Cannot update. Department with ID {departmentDto.Id} not found." });
            }
            return Ok(new { message = "Department updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _service.DeleteDepartmentAsync(id);

            return result switch
            {
                DepartmentDeleteResult.NotFound => NotFound(new { message = $"Cannot delete. Department with ID {id} not found." }),
                DepartmentDeleteResult.HasEmployees => Conflict(new { message = "This department cannot be deleted because it is currently assigned to one or more employees." }),
                _ => Ok(new { message = "Department deleted successfully!" })
            };
        }
    }
}