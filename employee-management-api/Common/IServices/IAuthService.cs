using Common.Dto;

namespace Common.IServices
{
    public interface IAuthService
    {
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
    }
}