using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Ascon.Pilot.Core
{
    public static class ProtectedDataEx
    {
        private static readonly byte[] Salt = Encoding.Unicode.GetBytes("PrO!EcTedD@taS@Lt&KeY");

        public static byte[] Protect(this SecureString input)
        {
            if (input == null)
                return new byte[] { };

            using (var aes = Aes.Create())
            {
                aes.Key = Salt;
                var encryptor = aes.CreateEncryptor();
                var bytesToEncrypt = Encoding.Unicode.GetBytes(input.ConvertToUnsecureString());
                return encryptor.TransformFinalBlock(bytesToEncrypt, 0, bytesToEncrypt.Length);
            }
        }

        public static SecureString Unprotect(this byte[] encryptedData)
        {
            try
            {
                byte[] decryptedData;
                using (var aes = Aes.Create())
                {
                    aes.Key = Salt;
                    var decryptor = aes.CreateDecryptor();
                    decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                }
                return Encoding.Unicode.GetString(decryptedData, 0, decryptedData.Length).ConvertToSecureString();
            }
            catch (CryptographicException)
            {
                return new SecureString();
            }
        }
    }
}