using System;
using System.Security.Cryptography;

namespace Ascon.Pilot.Core
{
    public static class PasswordEx
    {
        private const int SALT_BYTE_SIZE = 24;
        private const int HASH_BYTE_SIZE = 24;
        private const int PBKDF2_ITERATIONS = 1000;

        private const int ITERATION_INDEX = 0;
        private const int SALT_INDEX = 1;
        private const int PBKDF2_INDEX = 2;

        private const char DELIMITER = ':' ;

        public static string CreateHash(this string password)
        {
            var salt = RandomSalt();
            var hash = Pbkdf2(password, salt, PBKDF2_ITERATIONS, HASH_BYTE_SIZE);

            return string.Format("{0}{1}{2}{3}{4}", PBKDF2_ITERATIONS, DELIMITER, Convert.ToBase64String(salt), DELIMITER, Convert.ToBase64String(hash)); 
        }

        private static byte[] RandomSalt()
        {
            var salt = new byte[SALT_BYTE_SIZE];
            using (var csprng = RandomNumberGenerator.Create())
                csprng.GetBytes(salt);
            return salt;
        }

        private static byte[] Pbkdf2(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
                return pbkdf2.GetBytes(outputBytes);
        }

        public static bool ValidatePassword(this string password, string correctHash)
        {
            try
            {
                string[] split = correctHash.Split(DELIMITER);
                int iterations = Int32.Parse(split[ITERATION_INDEX]);
                byte[] salt = Convert.FromBase64String(split[SALT_INDEX]);
                byte[] hash = Convert.FromBase64String(split[PBKDF2_INDEX]);

                byte[] testHash = Pbkdf2(password, salt, iterations, hash.Length);
                return SlowEquals(hash, testHash);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Compares two byte arrays in length-constant time. This comparison
        /// method is used so that password hashes cannot be extracted from
        /// on-line systems using a timing attack and then attacked off-line.
        /// </summary>
        /// <param name="a">The first byte array.</param>
        /// <param name="b">The second byte array.</param>
        /// <returns>True if both byte arrays are equal. False otherwise.</returns>
        private static bool SlowEquals(byte[] a, byte[] b)
        {
            uint diff = (uint)a.Length ^ (uint)b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= (uint)(a[i] ^ b[i]);
            return diff == 0;
        }
    }
}