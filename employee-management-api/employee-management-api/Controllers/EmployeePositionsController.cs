using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Common.Dto;
using Common.IServices;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeePositionsController : ControllerBase
    {
        private readonly IEmployeePositionService _service;
        public EmployeePositionsController(IEmployeePositionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<EmployeePositionDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            [FromQuery] string? search = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _service.GetHistoryAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Assign(AssignPositionDto dto)
        {
            var createdBy = User.Identity?.Name;
            var created = await _service.AssignPositionAsync(dto, createdBy);
            return Ok(new
            {
                message = "Position assigned successfully!",
                employeePosition = created
            });
        }
    }
}