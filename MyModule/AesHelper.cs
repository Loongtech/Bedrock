using System.Security.Cryptography;
using System.Text;

namespace Net.LoongTech.OmniCoreX
{
    public static class AesHelper
    {
        private static readonly byte[] _defaultKey = new byte[16];
        private static readonly byte[] _defaultIV = new byte[16];

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key"></param>
        /// <returns>密文</returns>
        public static string Encrypt(string plainText, string key = "LoongTech.Net")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = _defaultIV;
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using Aes aes = Aes.Create();
            aes.KeySize = 128;
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
            cryptoStream.FlushFinalBlock();

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="cipherText">密文</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherText, string key = "LoongTech.Net")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = _defaultIV;
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.KeySize = 128;
            aes.Key = keyBytes;
            aes.IV = ivBytes;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Write);

            cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);
            cryptoStream.FlushFinalBlock();

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
