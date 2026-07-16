using System.Security.Cryptography;

namespace EmployeeManagementAPI.Helpers
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string hashed)
        {
            var parts = hashed.Split('.', 2);
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var key = Convert.FromBase64String(parts[1]);

            var attemptedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
            return CryptographicOperations.FixedTimeEquals(key, attemptedKey);
        }

        //  password from the employee's ID

        public static string GenerateFromEmployeeId(int employeeId)
        {
            return $"Emp{employeeId}@2026";
        }
    }
}