using System.Security.Cryptography;
using System.Text;

namespace Net.LoongTech.Bedrock.Security
{
    /// <summary>
    /// 提供基于 AES (Advanced Encryption Standard) 的加密和解密功能。
    /// 采用 CBC 模式和 PKCS7 填充，并为每次加密生成随机的IV，以确保安全性。
    /// </summary>
    public static class AesEncryption
    {
        private const int KeySize = 128; // AES-128
        private const int BlockSize = 128; // AES block size is always 128 bits
        private const int IvSizeInBytes = BlockSize / 8; // 16 bytes

        /// <summary>
        /// 加密一段明文字符串。
        /// </summary>
        /// <param name="plainText">要加密的明文。</param>
        /// <param name="key">加密密钥。必须是16字节（128位）的Base64编码字符串。</param>
        /// <returns>一个组合了IV和密文的Base64编码字符串，格式为 "IV:CipherText"。</returns>
        /// <exception cref="ArgumentException">当密钥无效时抛出。</exception>
        public static string Encrypt(string plainText, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            if (keyBytes.Length * 8 != KeySize)
            {
                throw new ArgumentException($"密钥长度无效。AES-128需要一个{KeySize}-bit (16-byte)的密钥。", nameof(key));
            }

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.Mode = CipherMode.CBC; // 使用更安全的CBC模式
            aes.Padding = PaddingMode.PKCS7;

            // --- 关键安全改进：为每次加密生成一个新的、随机的IV ---
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var memoryStream = new MemoryStream();
            // 1. 先将IV写入流的开头
            memoryStream.Write(iv, 0, iv.Length);

            // 2. 再写入加密后的密文
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        /// <summary>
        /// 解密一段密文字符串。
        /// </summary>
        /// <param name="cipherTextWithIv">包含IV和密文的Base64编码字符串。</param>
        /// <param name="key">解密密钥。必须是16字节（128位）的Base64编码字符串。</param>
        /// <returns>解密后的明文字符串。</returns>
        public static string Decrypt(string cipherTextWithIv, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            if (keyBytes.Length * 8 != KeySize)
            {
                throw new ArgumentException($"密钥长度无效。AES-128需要一个{KeySize}-bit (16-byte)的密钥。", nameof(key));
            }

            byte[] cipherBytesWithIv = Convert.FromBase64String(cipherTextWithIv);
            if (cipherBytesWithIv.Length < IvSizeInBytes)
            {
                throw new ArgumentException("密文格式无效，长度不足以包含IV。", nameof(cipherTextWithIv));
            }

            // --- 从数据流中提取IV ---
            byte[] iv = new byte[IvSizeInBytes];
            Array.Copy(cipherBytesWithIv, 0, iv, 0, iv.Length);

            using var aes = Aes.Create();
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var memoryStream = new MemoryStream();

            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
            {
                // 只将密文部分写入解密流
                cryptoStream.Write(cipherBytesWithIv, iv.Length, cipherBytesWithIv.Length - iv.Length);
                cryptoStream.FlushFinalBlock();
            }

            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
