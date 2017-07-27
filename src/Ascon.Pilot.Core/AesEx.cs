using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Ascon.Pilot.Core
{
    public static class AesEx
    {
        private static readonly byte[] Password = Encoding.Unicode.GetBytes("123456");
        private static readonly byte[] Salt = Encoding.Unicode.GetBytes("{5AF2954F-02FC-445A-BC27-976DD11A6258}");
        const int ITERATIONS = 1000;

        public static string EncryptAes(this string plainText)
        {
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                using (DeriveBytes rgb = new Rfc2898DeriveBytes(Password, Salt, ITERATIONS))
                {
                    aesAlg.Key = rgb.GetBytes(aesAlg.KeySize >> 3);
                    aesAlg.IV = rgb.GetBytes(aesAlg.BlockSize >> 3);

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptAes(this string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            string plaintext;

            using (Aes aesAlg = Aes.Create())
            {
                using (DeriveBytes rgb = new Rfc2898DeriveBytes(Password, Salt, ITERATIONS))
                {
                    aesAlg.Key = rgb.GetBytes(aesAlg.KeySize >> 3);
                    aesAlg.IV = rgb.GetBytes(aesAlg.BlockSize >> 3);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }

            return plaintext;
        }
    }
}