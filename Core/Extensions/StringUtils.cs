// 命名空间：Net.LoongTech.Bedrock.Core.Extensions
using System.Text;
using System.Text.RegularExpressions;

namespace Net.LoongTech.Bedrock.Core.Extensions
{
    /// <summary>
    /// 提供一系列强大的字符串处理静态扩展方法。
    /// </summary>
    public static class StringUtils
    {
        // 用于生成随机字符串的静态Random实例，避免在方法内重复创建导致伪随机问题。
        private static readonly Random _random = new Random();

        /// <summary>
        /// 去除字符串中的HTML标签，并清理一些非标准标记。
        /// </summary>
        /// <param name="input">包含HTML的输入字符串。</param>
        /// <param name="maxLength">如果大于0，则将结果截断到指定长度。</param>
        /// <returns>纯文本字符串。</returns>
        public static string ReplaceHtmlTag(this string input, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // 1. 使用正则表达式高效移除标准HTML标签和实体
            string strText = Regex.Replace(input, "<[^>]+>", "");
            strText = Regex.Replace(strText, "&[^;]+;", "");

            // 2. 移除一些特定的、非标准的硬编码字符串（可以根据需要调整）
            // 注意：这种方式比较脆弱，如果可能，应寻找更通用的清理规则。
            strText = strText.Replace("<span style=\"colo", "")
                             .Replace("-</p", "").Replace("//</p", "")
                             .Replace("！！！！", "").Replace("</d", "")
                             .Replace("<span", "").Replace("</span", "")
                             .Replace("<p", "").Replace("</p", "").Replace("</s", "");

            // 3. 移除所有空白字符
            strText = Regex.Replace(strText, @"\s", "");

            // 4. 按需截断
            if (maxLength > 0 && strText.Length > maxLength)
                return strText.Substring(0, maxLength);

            return strText;
        }

        /// <summary>
        /// 将字符串按GBK编码的字节长度进行裁剪，确保不超过指定的最大字节数。
        /// 注意：需要项目引用 System.Text.Encoding.CodePages 包。
        /// </summary>
        /// <param name="input">原始字符串。</param>
        /// <param name="maxBytes">最大字节长度。</param>
        /// <param name="appendEllipsis">如果裁剪发生，是否在末尾添加 "..."。</param>
        /// <returns>裁剪后的字符串。</returns>
        public static string TruncateByBytes(this string input, int maxBytes, bool appendEllipsis = false)
        {
            if (string.IsNullOrEmpty(input)) return input;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("GBK");

            if (encoding.GetByteCount(input) <= maxBytes)
            {
                return input;
            }

            string ellipsis = appendEllipsis ? "..." : string.Empty;
            int ellipsisByteLength = encoding.GetByteCount(ellipsis);
            int currentLength = input.Length;

            // 循环递减字符串长度，直到其字节数符合要求
            while (currentLength > 0 && encoding.GetByteCount(input.Substring(0, currentLength)) > maxBytes - ellipsisByteLength)
            {
                currentLength--;
            }

            return input.Substring(0, currentLength) + ellipsis;
        }

        /// <summary>
        /// 提取字符串中首次出现的开始字符与最后一次出现的结束字符之间的内容。
        /// </summary>
        /// <param name="input">原始字符串。</param>
        /// <param name="startChar">开始字符。</param>
        /// <param name="endChar">结束字符。</param>
        /// <returns>提取的子字符串，如果未找到则为空字符串。</returns>
        public static string GetContentBetween(this string input, char startChar, char endChar)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            int firstIndex = input.IndexOf(startChar);
            int lastIndex = input.LastIndexOf(endChar);

            if (firstIndex != -1 && lastIndex != -1 && lastIndex > firstIndex)
            {
                return input.Substring(firstIndex + 1, lastIndex - (firstIndex + 1));
            }
            return string.Empty;
        }

        /// <summary>
        /// 使用正则表达式提取两个指定子字符串之间的内容。
        /// </summary>
        /// <param name="input">原始字符串。</param>
        /// <param name="startStr">开始子字符串。</param>
        /// <param name="endStr">结束子字符串。</param>
        /// <returns>匹配到的第一个结果。</returns>
        public static string GetContentBetween(this string input, string startStr, string endStr)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // 对传入的边界字符串进行转义，以防它们包含正则表达式的特殊字符
            string escapedStart = Regex.Escape(startStr);
            string escapedEnd = Regex.Escape(endStr);

            var rg = new Regex($"(?<={escapedStart})[\\s\\S]*?(?={escapedEnd})", RegexOptions.Multiline);
            return rg.Match(input).Value;
        }

        /// <summary>
        /// 生成指定长度的随机字符串（包含大小写字母和数字）。
        /// </summary>
        /// <param name="length">随机字符串的长度。</param>
        /// <returns>生成的随机字符串。</returns>
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[_random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        /// <summary>
        /// 根据GBK编码，在字符串右侧填充空格以达到指定的总字节数（实现左对齐）。
        /// </summary>
        /// <param name="input">原始字符串。</param>
        /// <param name="totalBytes">目标总字节数。</param>
        /// <returns>填充后的字符串。</returns>
        public static string PadRightBytes(this string input, int totalBytes)
        {
            //获取GBK编码
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding coding = Encoding.GetEncoding("GBK");

            //计算字符串长度
            int dcount = 0;
            foreach (char ch in input.ToCharArray())
            {
                //如果字符串中字符个数为2，则计数加1
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            //截取字符串，截取长度为总字节数减去计数
            string w = input.PadRight(totalBytes - dcount);
            //返回截取后的字符串
            return w;
        }

        /// <summary>
        /// 根据GBK编码，在字符串左侧填充空格以达到指定的总字节数（实现右对齐）。
        /// </summary>
        /// <param name="input">原始字符串。</param>
        /// <param name="totalBytes">目标总字节数。</param>
        /// <returns>填充后的字符串。</returns>
        public static string PadLeftBytes(this string input, int totalBytes)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding coding = Encoding.GetEncoding("GBK");

            int dcount = 0;
            foreach (char ch in input.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            string w = input.PadLeft(totalBytes - dcount);
            return w;
        }
    }
}