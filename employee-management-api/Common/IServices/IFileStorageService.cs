namespace Common.IServices
{
    public interface IFileStorageService
    {
        Task<string> SaveEmployeePhotoAsync(int employeeId, Stream fileStream, string fileExtension);
        void DeleteEmployeePhoto(string? relativePath);
    }
}