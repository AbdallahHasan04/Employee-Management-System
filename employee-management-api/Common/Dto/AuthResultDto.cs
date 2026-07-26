namespace Common.Dto
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsLockedOut { get; set; }
        public int LockoutRemainingSeconds { get; set; }
    }
}