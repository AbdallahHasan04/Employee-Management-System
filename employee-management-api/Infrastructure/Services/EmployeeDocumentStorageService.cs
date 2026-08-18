using Common.IServices;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class EmployeeDocumentStorageService : IEmployeeDocumentStorageService
    {
        private readonly string _employeeDocumentsRoot;

        public EmployeeDocumentStorageService(IConfiguration config)
        {
            var relativeRoot = config["FileStorage:EmployeeDocumentsPath"] ?? "App_Data/employee-documents";
            _employeeDocumentsRoot = Path.Combine(Directory.GetCurrentDirectory(), relativeRoot);
            Directory.CreateDirectory(_employeeDocumentsRoot);
        }

        public async Task<string> SaveDocumentAsync(int employeeId, Stream fileStream, string fileExtension)
        {
            var fileName = $"{employeeId}_{Guid.NewGuid():N}{fileExtension}";
            var fullPath = Path.Combine(_employeeDocumentsRoot, fileName);

            await using (var output = File.Create(fullPath))
            {
                await fileStream.CopyToAsync(output);
            }

            return fileName;
        }

        public string GetPhysicalPath(string relativePath)
        {
            var fileName = Path.GetFileName(relativePath);
            return Path.Combine(_employeeDocumentsRoot, fileName);
        }

        public bool DocumentExists(string relativePath)
        {
            return File.Exists(GetPhysicalPath(relativePath));
        }

        public void DeleteDocument(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var fullPath = GetPhysicalPath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}