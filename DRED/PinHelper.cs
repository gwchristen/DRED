using System;
using System.Security.Cryptography;
using System.Text;

namespace DRED
{
    public static class PinHelper
    {
        public const int Sha256HexLength = 64;

        /// <summary>Returns the SHA256 hex hash of the given PIN string.</summary>
        public static string HashPin(string pin)
        {
            byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(pin ?? string.Empty));
            return Convert.ToHexString(bytes);
        }

        /// <summary>Returns true if the entered PIN's hash matches the stored hash.</summary>
        public static bool VerifyPin(string enteredPin, string storedHash)
        {
            string enteredHash = HashPin(enteredPin);
            // Use constant-time comparison to prevent timing attacks
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(enteredHash),
                Encoding.UTF8.GetBytes(storedHash ?? string.Empty));
        }
    }
}
