using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Common.Dto;
using Common.IServices;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeeDocumentsController : ControllerBase
    {
        private readonly IEmployeeDocumentService _service;

        private static readonly string[] AllowedContentTypes =
        {
            "application/pdf",
            "image/jpeg", "image/png", "image/webp",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

        public EmployeeDocumentsController(IEmployeeDocumentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<EmployeeDocumentDto>>> Get(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false,
            [FromQuery] string? search = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var result = await _service.GetAllDocumentsAsync(pageNumber, pageSize, sortBy, sortDescending, search);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromForm] EmployeeDocumentUploadDto dto, IFormFile attachment)
        {
            if (dto.EmployeeId <= 0)
            {
                return BadRequest(new { message = "Please select an employee." });
            }

            if (string.IsNullOrWhiteSpace(dto.DocumentName))
            {
                return BadRequest(new { message = "Document name is required." });
            }

            if (attachment == null || attachment.Length == 0)
            {
                return BadRequest(new { message = "Please attach a file." });
            }

            if (attachment.Length > MaxFileSizeBytes)
            {
                return BadRequest(new { message = "Attachment must be 10MB or smaller." });
            }

            if (!AllowedContentTypes.Contains(attachment.ContentType))
            {
                return BadRequest(new { message = "Only PDF, Word, JPEG, PNG, and WEBP files are allowed." });
            }

            if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value.Date < dto.IssueDate.Date)
            {
                return BadRequest(new { message = "Expiry date cannot be earlier than the issue date." });
            }

            var extension = Path.GetExtension(attachment.FileName);
            if (string.IsNullOrWhiteSpace(extension))
            {
                extension = attachment.ContentType switch
                {
                    "application/pdf" => ".pdf",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "application/msword" => ".doc",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                    _ => ".jpg"
                };
            }

            var createdBy = User.Identity?.Name;

            await using var stream = attachment.OpenReadStream();
            var created = await _service.UploadDocumentAsync(dto, stream, extension, createdBy);

            return Ok(new
            {
                message = "Document uploaded successfully!",
                document = created
            });
        }

        [HttpGet("{id}/download")]
        public async Task<IActionResult> Download(int id)
        {
            var (result, physicalPath, fileName) = await _service.GetDocumentFileForDownloadAsync(id);

            if (result == DocumentDownloadResult.NotFound)
            {
                return NotFound(new { message = $"Document with id {id} not found." });
            }

            if (result == DocumentDownloadResult.FileMissing)
            {
                return NotFound(new { message = "This document's file could not be found on the server." });
            }

            if (!ContentTypeProvider.TryGetContentType(physicalPath!, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(physicalPath!, contentType, fileName);
        }
    }
}