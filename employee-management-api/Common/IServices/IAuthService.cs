using Common.Dto;

namespace Common.IServices
{
    public enum ChangePasswordResult
    {
        Success,
        InvalidCurrentPassword,
        UserNotFound
    }

    public interface IAuthService
    {
        Task<AuthResultDto> LoginAsync(LoginDto loginDto, string ipAddress);
        Task<ChangePasswordResult> ChangePasswordAsync(string username, ChangePasswordDto dto);
    }
}