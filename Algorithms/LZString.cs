using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.LoongTech.Bedrock.Algorithms
{
    /// <summary>
    /// 利用 lz-string 算法对字符串进行压缩和解压缩的工具类。
    /// 适用于文本数据的高效压缩与还原。
    /// 参考：
    /// https://npm.io/package/lz-string
    /// https://github.com/pieroxy/lz-string/blob/c58a22021000ac2d99377cc0bf9ac193a12563c5/libs/lz-string.js
    /// </summary>
    public static class LZString
    {
        /// <summary>
        /// Base64编码使用的字符集，用于标准Base64编码
        /// </summary>
        private const string KeyStrBase64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        /// <summary>
        /// URL安全的Base64字符集，替换了标准Base64中的特殊字符
        /// </summary>
        private const string KeyStrUriSafe = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-$";

        /// <summary>
        /// 标准Base64字符到索引的映射字典
        /// </summary>
        private static readonly IDictionary<char, char> KeyStrBase64Dict = CreateBaseDict(KeyStrBase64);

        /// <summary>
        /// URL安全字符到索引的映射字典
        /// </summary>
        private static readonly IDictionary<char, char> KeyStrUriSafeDict = CreateBaseDict(KeyStrUriSafe);

        /// <summary>
        /// 创建字符到索引的映射字典
        /// </summary>
        private static IDictionary<char, char> CreateBaseDict(string alphabet)
        {
            var dict = new Dictionary<char, char>();
            for (var i = 0; i < alphabet.Length; i++)
            {
                dict[alphabet[i]] = (char)i;
            }
            return dict;
        }

        /// <summary>
        /// 将字符串压缩为Base64格式
        /// </summary>
        /// <param name="input">要压缩的字符串</param>
        /// <returns>Base64编码的压缩结果</returns>
        public static string CompressToBase64(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            var res = Compress(input, 6, code => KeyStrBase64[code]);
            // 补充标准Base64填充字符
            switch (res.Length % 4)
            {
                case 0: return res;
                case 1: return res + "===";
                case 2: return res + "==";
                case 3: return res + "=";
                default: throw new InvalidOperationException("不应该出现的情况");
            }
        }

        /// <summary>
        /// 解压缩Base64编码的字符串
        /// </summary>
        /// <param name="input">经Base64编码的压缩字符串 </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>

        public static string DecompressFromBase64(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return Decompress(input.Length, 32, index => KeyStrBase64Dict[input[index]]);
        }

        /// <summary>
        /// 将字符串压缩为UTF16编码格式
        /// </summary>
        public static string CompressToUTF16(string input)
        {
            return Compress(input, 15, code => (char)(code + 32));
        }

        public static string DecompressFromUTF16(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return Decompress(input.Length, 16384, index => (char)(input[index] - 32));
        }

        /// <summary>
        /// 将字符串压缩为URL安全的格式
        /// </summary>
        public static string CompressToEncodedURIComponent(string input)
        {
            if (input == null) return "";
            return Compress(input, 6, code => KeyStrUriSafe[code]);
        }

        public static string DecompressFromEncodedURIComponent(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            input = input.Replace(" ", "+");
            return Decompress(input.Length, 32, index => KeyStrUriSafeDict[input[index]]);
        }

        public static byte[] CompressToUint8Array(string uncompressed)
        {
            string compressed = Compress(uncompressed);
            byte[] buf = new byte[compressed.Length * 2];

            for (int i = 0; i < compressed.Length; i++)
            {
                int current_value = Convert.ToInt32(compressed[i]);
                buf[i * 2] = (byte)(current_value >> 8);
                buf[i * 2 + 1] = (byte)(current_value % 256);
            }
            return buf;
        }

        public static string DecompressFromUint8Array(byte[] compressed)
        {
            if (compressed == null) throw new ArgumentNullException(nameof(compressed));

            char[] result = new char[compressed.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Convert.ToChar(compressed[i * 2] * 256 + compressed[i * 2 + 1]);
            }

            return Decompress(new string(result));
        }

        public static string Compress(string uncompressed)
        {
            return Compress(uncompressed, 16, code => (char)code);
        }
        /// <summary>
        /// 核心压缩算法实现
        /// 使用LZ78变体算法，通过动态字典构建实现数据压缩
        /// </summary>
        /// <param name="uncompressed">要压缩的原始字符串</param>
        /// <param name="bitsPerChar">每个输出字符的位数</param>
        /// <param name="getCharFromInt">整数到字符的转换函数</param>
        private static string Compress(string uncompressed, int bitsPerChar, Func<int, char> getCharFromInt)
        {
            if (uncompressed == null) throw new ArgumentNullException(nameof(uncompressed));

            int i, value;
            // 初始化压缩上下文
            var context_dictionary = new Dictionary<string, int>();      // 字符串到编码的映射字典
            var context_dictionaryToCreate = new Dictionary<string, bool>(); // 待创建的字典项

            var context_wc = "";
            var context_w = "";
            var context_enlargeIn = 2; // 字典扩展计数器
            var context_dictSize = 3;  // 当前字典大小
            var context_numBits = 2;   // 当前编码位数
            var context_data = new StringBuilder();
            var context_data_val = 0;
            var context_data_position = 0;

            foreach (var context_c in uncompressed)
            {
                if (!context_dictionary.ContainsKey(context_c.ToString()))
                {
                    context_dictionary[context_c.ToString()] = context_dictSize++;
                    context_dictionaryToCreate[context_c.ToString()] = true;
                }

                context_wc = context_w + context_c;
                if (context_dictionary.ContainsKey(context_wc))
                {
                    context_w = context_wc;
                }
                else
                {
                    if (context_dictionaryToCreate.ContainsKey(context_w))
                    {
                        if (FirstOrDefault(context_w) < 256)
                        {
                            for (i = 0; i < context_numBits; i++)
                            {
                                context_data_val = (context_data_val << 1);
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                            }
                            value = FirstOrDefault(context_w);
                            for (i = 0; i < 8; i++)
                            {
                                context_data_val = (context_data_val << 1) | (value & 1);
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                                value = value >> 1;
                            }
                        }
                        else
                        {
                            value = 1;
                            for (i = 0; i < context_numBits; i++)
                            {
                                context_data_val = (context_data_val << 1) | value;
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                                value = 0;
                            }
                            value = FirstOrDefault(context_w);
                            for (i = 0; i < 16; i++)
                            {
                                context_data_val = (context_data_val << 1) | (value & 1);
                                if (context_data_position == bitsPerChar - 1)
                                {
                                    context_data_position = 0;
                                    context_data.Append(getCharFromInt(context_data_val));
                                    context_data_val = 0;
                                }
                                else
                                {
                                    context_data_position++;
                                }
                                value = value >> 1;
                            }
                        }
                        context_enlargeIn--;
                        if (context_enlargeIn == 0)
                        {
                            context_enlargeIn = (int)Math.Pow(2, context_numBits);
                            context_numBits++;
                        }
                        context_dictionaryToCreate.Remove(context_w);
                    }
                    else
                    {
                        value = context_dictionary[context_w];
                        for (i = 0; i < context_numBits; i++)
                        {
                            context_data_val = (context_data_val << 1) | (value & 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = value >> 1;
                        }


                    }
                    context_enlargeIn--;
                    if (context_enlargeIn == 0)
                    {
                        context_enlargeIn = (int)Math.Pow(2, context_numBits);
                        context_numBits++;
                    }
                    // Add wc to the dictionary.
                    context_dictionary[context_wc] = context_dictSize++;
                    context_w = context_c.ToString();
                }
            }

            // Output the code for w.
            if (context_w != "")
            {
                if (context_dictionaryToCreate.ContainsKey(context_w))
                {
                    if (FirstOrDefault(context_w) < 256)
                    {
                        for (i = 0; i < context_numBits; i++)
                        {
                            context_data_val = (context_data_val << 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                        }
                        value = FirstOrDefault(context_w);
                        for (i = 0; i < 8; i++)
                        {
                            context_data_val = (context_data_val << 1) | (value & 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = value >> 1;
                        }
                    }
                    else
                    {
                        value = 1;
                        for (i = 0; i < context_numBits; i++)
                        {
                            context_data_val = (context_data_val << 1) | value;
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = 0;
                        }
                        value = FirstOrDefault(context_w);
                        for (i = 0; i < 16; i++)
                        {
                            context_data_val = (context_data_val << 1) | (value & 1);
                            if (context_data_position == bitsPerChar - 1)
                            {
                                context_data_position = 0;
                                context_data.Append(getCharFromInt(context_data_val));
                                context_data_val = 0;
                            }
                            else
                            {
                                context_data_position++;
                            }
                            value = value >> 1;
                        }
                    }
                    context_enlargeIn--;
                    if (context_enlargeIn == 0)
                    {
                        context_enlargeIn = (int)Math.Pow(2, context_numBits);
                        context_numBits++;
                    }
                    context_dictionaryToCreate.Remove(context_w);
                }
                else
                {
                    value = context_dictionary[context_w];
                    for (i = 0; i < context_numBits; i++)
                    {
                        context_data_val = (context_data_val << 1) | (value & 1);
                        if (context_data_position == bitsPerChar - 1)
                        {
                            context_data_position = 0;
                            context_data.Append(getCharFromInt(context_data_val));
                            context_data_val = 0;
                        }
                        else
                        {
                            context_data_position++;
                        }
                        value = value >> 1;
                    }


                }
                context_enlargeIn--;
                if (context_enlargeIn == 0)
                {
                    // context_enlargeIn = (int)Math.Pow(2, context_numBits); // Unused. Kept for tracking changes with https://github.com/pieroxy/lz-string/blob/master/libs/lz-string.js#L295
                    context_numBits++;
                }
            }

            // Mark the end of the stream
            value = 2;
            for (i = 0; i < context_numBits; i++)
            {
                context_data_val = (context_data_val << 1) | (value & 1);
                if (context_data_position == bitsPerChar - 1)
                {
                    context_data_position = 0;
                    context_data.Append(getCharFromInt(context_data_val));
                    context_data_val = 0;
                }
                else
                {
                    context_data_position++;
                }
                value = value >> 1;
            }

            // Flush the last char
            while (true)
            {
                context_data_val = (context_data_val << 1);
                if (context_data_position == bitsPerChar - 1)
                {
                    context_data.Append(getCharFromInt(context_data_val));
                    break;
                }
                else context_data_position++;
            }
            return context_data.ToString();
        }

        public static string Decompress(string compressed)
        {
            if (compressed == null) throw new ArgumentNullException(nameof(compressed));

            //TODO: Use an enumerator
            return Decompress(compressed.Length, 32768, index => compressed[index]);
        }

        /// <summary>
        /// 核心解压算法
        /// 通过重建压缩时的字典来还原原始数据
        /// </summary>
        private static string Decompress(int length, int resetValue, Func<int, char> getNextValue)
        {
            var dictionary = new List<string>();
            var enlargeIn = 4;
            var numBits = 3;
            string entry;
            var result = new StringBuilder();
            int i;
            string w;
            int bits = 0, resb, maxpower, power;
            var c = '\0';

            var data_val = getNextValue(0);
            var data_position = resetValue;
            var data_index = 1;

            for (i = 0; i < 3; i += 1)
            {
                dictionary.Add(((char)i).ToString());
            }

            maxpower = (int)Math.Pow(2, 2);
            power = 1;
            while (power != maxpower)
            {
                resb = data_val & data_position;
                data_position >>= 1;
                if (data_position == 0)
                {
                    data_position = resetValue;
                    data_val = getNextValue(data_index++);
                }
                bits |= (resb > 0 ? 1 : 0) * power;
                power <<= 1;
            }

            switch (bits)
            {
                case 0:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 8);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data_val & data_position;
                        data_position >>= 1;
                        if (data_position == 0)
                        {
                            data_position = resetValue;
                            data_val = getNextValue(data_index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c = (char)bits;
                    break;
                case 1:
                    bits = 0;
                    maxpower = (int)Math.Pow(2, 16);
                    power = 1;
                    while (power != maxpower)
                    {
                        resb = data_val & data_position;
                        data_position >>= 1;
                        if (data_position == 0)
                        {
                            data_position = resetValue;
                            data_val = getNextValue(data_index++);
                        }
                        bits |= (resb > 0 ? 1 : 0) * power;
                        power <<= 1;
                    }
                    c = (char)bits;
                    break;
                case 2:
                    return "";
            }
            w = c.ToString();
            dictionary.Add(w);
            result.Append(c);
            while (true)
            {
                if (data_index > length)
                {
                    return "";
                }

                bits = 0;
                maxpower = (int)Math.Pow(2, numBits);
                power = 1;
                while (power != maxpower)
                {
                    resb = data_val & data_position;
                    data_position >>= 1;
                    if (data_position == 0)
                    {
                        data_position = resetValue;
                        data_val = getNextValue(data_index++);
                    }
                    bits |= (resb > 0 ? 1 : 0) * power;
                    power <<= 1;
                }

                int c2;
                switch (c2 = bits)
                {
                    case (char)0:
                        bits = 0;
                        maxpower = (int)Math.Pow(2, 8);
                        power = 1;
                        while (power != maxpower)
                        {
                            resb = data_val & data_position;
                            data_position >>= 1;
                            if (data_position == 0)
                            {
                                data_position = resetValue;
                                data_val = getNextValue(data_index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }

                        c2 = dictionary.Count;
                        dictionary.Add(((char)bits).ToString());
                        enlargeIn--;
                        break;
                    case (char)1:
                        bits = 0;
                        maxpower = (int)Math.Pow(2, 16);
                        power = 1;
                        while (power != maxpower)
                        {
                            resb = data_val & data_position;
                            data_position >>= 1;
                            if (data_position == 0)
                            {
                                data_position = resetValue;
                                data_val = getNextValue(data_index++);
                            }
                            bits |= (resb > 0 ? 1 : 0) * power;
                            power <<= 1;
                        }
                        c2 = dictionary.Count;
                        dictionary.Add(((char)bits).ToString());
                        enlargeIn--;
                        break;
                    case (char)2:
                        return result.ToString();
                }

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }

                if (dictionary.Count - 1 >= c2)
                {
                    entry = dictionary[c2];
                }
                else
                {
                    if (c2 == dictionary.Count)
                    {
                        entry = w + w[0];
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                result.Append(entry);

                // Add w+entry[0] to the dictionary.
                dictionary.Add(w + entry[0]);
                enlargeIn--;

                w = entry;

                if (enlargeIn == 0)
                {
                    enlargeIn = (int)Math.Pow(2, numBits);
                    numBits++;
                }
            }
        }
        /// <summary>
        /// 获取字符串的第一个字符，为空则返回默认值
        /// </summary>
        private static char FirstOrDefault(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(char);
            }

            return value[0];
        }
    }
}
