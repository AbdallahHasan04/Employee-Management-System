using Common.Dto;

namespace Common.IServices
{
    public enum DocumentDownloadResult
    {
        Success,
        NotFound,
        FileMissing,
        Expired
    }

    public enum DocumentDeleteResult
    {
        Success,
        NotFound,
        NotYetExpired
    }

    public interface IEmployeeDocumentService
    {
        Task<PagedResultDto<EmployeeDocumentDto>> GetAllDocumentsAsync(int pageNumber, int pageSize, string? sortBy, bool sortDescending, string? search);
        Task<EmployeeDocumentDto> UploadDocumentAsync(EmployeeDocumentUploadDto dto, Stream fileStream, string fileExtension, string? createdBy);
        Task<(DocumentDownloadResult Result, string? PhysicalPath, string? FileName)> GetDocumentFileForDownloadAsync(int id);
        Task<DocumentDeleteResult> DeleteDocumentAsync(int id);
    }
}