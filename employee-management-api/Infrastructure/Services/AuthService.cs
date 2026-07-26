using Common.Dto;
using Common.IRepository;
using Common.IServices;
using Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;
        private readonly LoginAttemptTracker _attemptTracker;

        public AuthService(IUserRepository userRepository, IConfiguration config, LoginAttemptTracker attemptTracker)
        {
            _userRepository = userRepository;
            _config = config;
            _attemptTracker = attemptTracker;
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto loginDto, string ipAddress)
        {
            if (_attemptTracker.IsLockedOut(ipAddress, out var remaining))
            {
                return new AuthResultDto
                {
                    Success = false,
                    IsLockedOut = true,
                    LockoutRemainingSeconds = (int)Math.Ceiling(remaining.TotalSeconds)
                };
            }

            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

            if (user == null || user.Status != "Active" || !PasswordHasher.Verify(loginDto.Password, user.Password))
            {
                _attemptTracker.RecordFailure(ipAddress);
                return new AuthResultDto { Success = false };
            }

            _attemptTracker.RecordSuccess(ipAddress);

            var jwtSettings = _config.GetSection("Jwt");
            var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var minutes) ? minutes : 30;
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = GenerateJwtToken(user.Username, expiresAt, jwtSettings);

            return new AuthResultDto { Success = true, Token = token, ExpiresAt = expiresAt };
        }

        private static string GenerateJwtToken(string username, DateTime expiresAt, IConfigurationSection jwtSettings)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}