using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Common.Dto;
using Common.IServices;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _service;
        public PositionsController(IPositionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<PositionDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            [FromQuery] string? search = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 1000) pageSize = 10;

            var result = await _service.GetAllPositionsAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionDto>> GetById(int id)
        {
            var position = await _service.GetPositionByIdAsync(id);
            if (position == null)
            {
                return NotFound(new { message = $"Position with id {id} not found" });
            }
            return Ok(position);
        }

        [HttpPost]
        public async Task<ActionResult> Create(PositionDto positionDto)
        {
            var createdBy = User.Identity?.Name;
            var created = await _service.CreatePositionAsync(positionDto, createdBy);
            return Ok(new
            {
                message = "Position created successfully!",
                position = created
            });
        }

        [HttpPut]
        public async Task<ActionResult> Update(PositionDto positionDto)
        {
            var modifiedBy = User.Identity?.Name;
            var updated = await _service.UpdatePositionAsync(positionDto, modifiedBy);
            if (!updated)
            {
                return NotFound(new { message = $"Cannot update. Position with ID {positionDto.Id} not found." });
            }
            return Ok(new { message = "Position updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var deletedBy = User.Identity?.Name;
            var result = await _service.DeletePositionAsync(id, deletedBy);

            return result switch
            {
                PositionDeleteResult.NotFound => NotFound(new { message = $"Cannot delete. Position with ID {id} not found." }),
                PositionDeleteResult.InUse => Conflict(new { message = "This position cannot be deleted because it is currently assigned to one or more employees." }),
                _ => Ok(new { message = "Position deleted successfully!" })
            };
        }
    }
}