namespace Common.IServices
{
    public interface IEmployeeDocumentStorageService
    {
        Task<string> SaveDocumentAsync(int employeeId, Stream fileStream, string fileExtension);
        string GetPhysicalPath(string relativePath);
        bool DocumentExists(string relativePath);
        void DeleteDocument(string? relativePath);
    }
}