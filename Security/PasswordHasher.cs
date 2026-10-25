using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AuthenticationModule.Security
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16, HashSize = 32, Iterations = 10000;
        private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

        public static (string hash, string salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = KeyDerivation.Pbkdf2(password, saltBytes, Prf, Iterations, HashSize);
            return (Convert.ToBase64String(hash), Convert.ToBase64String(saltBytes));
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            var computed = KeyDerivation.Pbkdf2(password, Convert.FromBase64String(storedSalt), Prf, Iterations, HashSize);
            return CryptographicOperations.FixedTimeEquals(computed, Convert.FromBase64String(storedHash));
        }
    }
}