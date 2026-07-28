using Common.IServices;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _employeePhotosRoot;

        public FileStorageService(IConfiguration config)
        {
            var relativeRoot = config["FileStorage:EmployeePhotosPath"] ?? "wwwroot/employee-photos";
            _employeePhotosRoot = Path.Combine(Directory.GetCurrentDirectory(), relativeRoot);
            Directory.CreateDirectory(_employeePhotosRoot);
        }

        public async Task<string> SaveEmployeePhotoAsync(int employeeId, Stream fileStream, string fileExtension)
        {
            var fileName = $"{employeeId}_{Guid.NewGuid():N}{fileExtension}";
            var fullPath = Path.Combine(_employeePhotosRoot, fileName);

            await using (var output = File.Create(fullPath))
            {
                await fileStream.CopyToAsync(output);
            }

            return $"employee-photos/{fileName}";
        }

        public void DeleteEmployeePhoto(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            var fileName = Path.GetFileName(relativePath);
            var fullPath = Path.Combine(_employeePhotosRoot, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}