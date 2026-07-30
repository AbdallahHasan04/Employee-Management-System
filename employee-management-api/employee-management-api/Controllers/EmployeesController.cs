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
        private readonly IFileStorageService _fileStorageService;

        private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

        public EmployeesController(IEmployeeService service, IFileStorageService fileStorageService)
        {
            _service = service;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<EmployeeDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            [FromQuery] string? search = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _service.GetAllEmployeesAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return Ok(result);
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
            var deletedBy = User.Identity?.Name;
            var deleted = await _service.DeleteEmployeeAsync(id, deletedBy);
            if (!deleted)
            {
                return NotFound(new { message = $"Cannot delete. Employee with ID {id} not found." });
            }
            return Ok(new { message = "Employee deleted successfully!" });
        }

        [HttpPost("{id}/photo")]
        public async Task<ActionResult> UploadPhoto(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file was uploaded." });
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return BadRequest(new { message = "Image must be 2MB or smaller." });
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return BadRequest(new { message = "Only JPEG, PNG, and WEBP images are allowed." });
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = file.ContentType switch
                {
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    _ => ".jpg"
                };
            }

            await using var stream = file.OpenReadStream();
            var relativePath = await _fileStorageService.SaveEmployeePhotoAsync(id, stream, extension);

            var updated = await _service.UpdateProfileImageAsync(id, relativePath);
            if (updated == null)
            {
                return NotFound(new { message = $"Employee with id {id} not found" });
            }

            return Ok(new { message = "Photo uploaded successfully!", employee = updated });
        }

        [HttpDelete("{id}/photo")]
        public async Task<ActionResult> RemovePhoto(int id)
        {
            var removed = await _service.RemoveProfileImageAsync(id);
            if (!removed)
            {
                return NotFound(new { message = $"Employee with id {id} not found" });
            }
            return Ok(new { message = "Photo removed successfully!" });
        }
    }
}