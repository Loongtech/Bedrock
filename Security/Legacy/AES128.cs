using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Net.LoongTech.Bedrock.Security.Legacy
{
    /// <summary>
    /// [警告] 提供基于 AES-128 ECB 模式的加密，存在严重安全风险。
    /// 此类仅为兼容历史数据而保留，新功能绝不应该使用。
    /// </summary>
    [Obsolete("AES128类使用了不安全的ECB模式和固定的IV，存在严重安全风险。请立即迁移到 AesEncryption 类。", error: false)]
    public static class AES128
    {
        private static readonly byte[] DefaultKey = new byte[16];
        private static readonly byte[] DefaultIV = new byte[16];

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <param name="key">密钥</param>
        /// <returns>密文</returns>
        public static string Encrypt(string plainText, string key = "ChengDu_QianLong")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = DefaultIV;
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
        /// <param name="key">密钥</param>
        /// <returns>明文</returns>
        public static string Decrypt(string cipherText, string key = "ChengDu_QianLong")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = DefaultIV;
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
