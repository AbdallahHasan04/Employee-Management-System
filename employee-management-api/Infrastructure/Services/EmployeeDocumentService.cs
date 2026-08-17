using AutoMapper;
using Common.Dto;
using Common.IRepository;
using Common.IServices;
using Core.Entities;

namespace Infrastructure.Services
{
    public class EmployeeDocumentService : IEmployeeDocumentService
    {
        private readonly IEmployeeDocumentRepository _documentRepository;
        private readonly IEmployeeDocumentStorageService _documentStorageService;
        private readonly IMapper _mapper;

        public EmployeeDocumentService(
            IEmployeeDocumentRepository documentRepository,
            IEmployeeDocumentStorageService documentStorageService,
            IMapper mapper)
        {
            _documentRepository = documentRepository;
            _documentStorageService = documentStorageService;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<EmployeeDocumentDto>> GetAllDocumentsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search)
        {
            var (items, totalCount) = await _documentRepository.GetPagedAsync(pageNumber, pageSize, sortBy, sortDescending, search);

            return new PagedResultDto<EmployeeDocumentDto>
            {
                Items = items.Select(d => _mapper.Map<EmployeeDocumentDto>(d)).ToList(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<EmployeeDocumentDto> UploadDocumentAsync(EmployeeDocumentUploadDto dto, Stream fileStream, string fileExtension, string? createdBy)
        {
            // Save the file first, if this throws, nothing has been written
            var relativePath = await _documentStorageService.SaveDocumentAsync(dto.EmployeeId, fileStream, fileExtension);

            var document = _mapper.Map<EmployeeDocument>(dto);
            document.DocumentPath = relativePath;
            document.IssueDate = document.IssueDate.Date;
            document.ExpiryDate = document.ExpiryDate?.Date;
            document.CreatedBy = createdBy;
            document.CreationDate = DateTime.UtcNow;

            await _documentRepository.AddAsync(document);

            var saved = await _documentRepository.GetByIdAsync(document.Id);
            return _mapper.Map<EmployeeDocumentDto>(saved!);
        }

        public async Task<(DocumentDownloadResult Result, string? PhysicalPath, string? FileName)> GetDocumentFileForDownloadAsync(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return (DocumentDownloadResult.NotFound, null, null);
            }

            if (!_documentStorageService.DocumentExists(document.DocumentPath))
            {
                return (DocumentDownloadResult.FileMissing, null, null);
            }

            var physicalPath = _documentStorageService.GetPhysicalPath(document.DocumentPath);
            return (DocumentDownloadResult.Success, physicalPath, document.DocumentName);
        }
    }
}