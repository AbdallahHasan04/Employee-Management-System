using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Common.Dto;
using Common.IServices;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
        {
            var summary = await _service.GetSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("employees-by-department")]
        public async Task<ActionResult<List<DepartmentEmployeeCountDto>>> GetEmployeesByDepartment()
        {
            var result = await _service.GetEmployeesByDepartmentAsync();
            return Ok(result);
        }
    }
}