using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Net.LoongTech.OmniCoreX
{
    public class StringHelper
    {
        #region 字符串处理
        /// <summary>
        /// 裁减字符串 - 直接裁减
        /// </summary>
        /// <param name="originalText">被裁减字符串</param>
        /// <param name="bytesAfterCut">需保留的字节数</param>
        /// <returns></returns>
        public string GetTreatedText(string? originalText, int bytesAfterCut)
        {
            if (originalText == null)
                return string.Empty;

            string treatedText = originalText;
            byte[] val = Encoding.Default.GetBytes(originalText);
            if (val.Length > bytesAfterCut)
            {
                treatedText = Encoding.Default.GetString(val, 0, bytesAfterCut);
            }
            return treatedText;
        }

        /// <summary>
        /// 在字符串的右侧填充空格，以实现左对齐
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="totalByteCount">长度</param>
        /// <returns></returns>
        public string PadRightEx(string str, int totalByteCount)
        {
            //获取GBK编码
            Encoding coding = Encoding.GetEncoding("GBK");

            //计算字符串长度
            int dcount = 0;
            foreach (char ch in str.ToCharArray())
            {
                //如果字符串中字符个数为2，则计数加1
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            //截取字符串，截取长度为总字节数减去计数
            string w = str.PadRight(totalByteCount - dcount);
            //返回截取后的字符串
            return w;
        }

        /// <summary>
        /// 在字符串的左侧填充空格，以实现右对齐
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="totalByteCount">长度</param>
        /// <returns></returns>
        public string PadLeftEx(string str, int totalByteCount)
        {
            Encoding coding = Encoding.GetEncoding("GBK");

            int dcount = 0;
            foreach (char ch in str.ToCharArray())
            {
                if (coding.GetByteCount(ch.ToString()) == 2)
                    dcount++;
            }
            string w = str.PadLeft(totalByteCount - dcount);
            return w;
        }
        /// <summary>
        /// 裁剪字符串(GBK)
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="maxLength">最大长度</param>
        /// <returns>裁剪后的字符串</returns>
        public string? TruncateToVarchar2Limit(string? input, int maxLength)
        {
            if (input != null)
            {
                // 注册GBK编码
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding encoding = Encoding.GetEncoding("GBK");

                if (string.IsNullOrEmpty(input) || encoding.GetByteCount(input) <= maxLength)
                {
                    return input;
                }

                while (encoding.GetByteCount(input) > maxLength)
                {
                    input = input.Substring(0, input.Length - 1);
                }
            }
            return input;
        }
        /// <summary>
        /// 裁减字符串
        /// </summary>
        /// <param name="originalText">被裁减字符串</param>
        /// <param name="bytesAfterCut">需保留的字节数</param>
        /// <returns></returns>
        public string GetOptimizedText(string originalText, int bytesAfterCut)
        {
            string optimizedText = originalText;

            byte[] val = Encoding.Default.GetBytes(originalText);
            if (val.Length > bytesAfterCut)
            {
                int left = bytesAfterCut / 2;
                int right = bytesAfterCut;
                left = left > originalText.Length ? originalText.Length : left;
                right = right > originalText.Length ? originalText.Length : right;
                while (left < right - 1)
                {
                    int mid = (left + right) / 2;
                    if (Encoding.Default.GetBytes(originalText.Substring(0, mid)).Length >
                        bytesAfterCut)
                    {
                        right = mid;
                    }
                    else
                    {
                        left = mid;
                    }
                }
                byte[] rightVal = Encoding.Default.GetBytes(originalText.Substring(0, right));
                if (rightVal.Length == bytesAfterCut)
                {
                    optimizedText = originalText.Substring(0, right - 3);
                }
                else
                {
                    optimizedText = originalText.Substring(0, left - 3);
                }
            }
            return optimizedText;
        }

        /// <summary>
        /// 将字符串转换为decimal类型的数值，并根据单位转换为相应的数值。
        /// </summary>
        /// <param name="issueString">需要转换的字符串。</param>
        /// <returns>转换后的decimal类型数值，如果转换失败则返回null。</returns>
        public decimal? ConvertToIndividual(string issueString)
        {
            decimal? returnValue = null;

            // 新的正则表达式，用来匹配数字和单位，同时考虑“万”和“亿”可能前置的情况
            Match match = Regex.Match(issueString, @"(?<number>\d+(\.\d+)?)(?<space>\s*)(?<unit>亿|万|千|元)");

            if (match.Success)
            {
                string numberStr = match.Groups["number"].Value;
                string unit = match.Groups["unit"].Value;

                decimal number;
                if (decimal.TryParse(numberStr, out number))
                {
                    // 特殊处理“万”和“亿”，将前置的数量级转换为实际数值
                    switch (unit)
                    {
                        case "亿":
                            number *= 100000000;
                            break;
                        case "万":
                            number *= 10000;
                            break;
                        case "千":
                            number *= 1000;
                            break;
                        case "元":
                            // 直接使用
                            break;
                        default: // 默认不处理
                            break;
                    }

                    returnValue = number;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length">随机字符串长度</param>
        /// <returns></returns>
        public string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            char[] stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        /// <summary>
        /// 获取字符串中的第一个字符和最后一个字符之间所有的字符串
        /// </summary>
        /// <param name="input">原始字符串</param>
        /// <param name="_beginChar">首次出现的 字符</param>
        /// <param name="_endChar">最后出现的 字符</param>
        /// <returns></returns>
        public string GetContentBetweenFirstAndLastChar(string input, char _beginChar, char _endChar)
        {
            int firstParenthesisIndex = input.IndexOf(_beginChar);
            int lastParenthesisIndex = input.LastIndexOf(_endChar);

            if (firstParenthesisIndex != -1 && lastParenthesisIndex != -1 && lastParenthesisIndex > firstParenthesisIndex)
            {
                int startIndex = firstParenthesisIndex + 1;
                int length = lastParenthesisIndex - startIndex;
                return input.Substring(startIndex, length);
            }

            return string.Empty;
        }

        /// <summary>
        /// 对字符串进行MD5加密
        /// </summary>
        /// <param name="sValue"></param>
        /// <returns></returns>
        public string MD5Encrypt(string sValue)
        {
            if (sValue == null || sValue.Length == 0)
                return sValue;

            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.Unicode.GetBytes(sValue);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// 截取两个指定字符串中间的数据
        /// </summary>
        /// <param name="sourse">原始字符串</param>
        /// <param name="startstr">开始字符串</param>
        /// <param name="endstr">接收字符串</param>
        /// <returns></returns>
        public string MidStr(string sourse, string startstr, string endstr)
        {
            Regex rg = new Regex("(?<=(" + startstr + "))[.\\s\\S]*?(?=(" + endstr + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(sourse).Value;
        }

        /// <summary>
        /// 获取文件的编码类型
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件的编码类型</returns>
        public Encoding GetEncoding(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                return GetEncoding(fs);
            }
        }

        /// <summary>
        /// 获取流文件的编码类型
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <returns>文件的编码类型</returns>
        public Encoding GetEncoding(Stream stream)
        {
            byte[] buffer = new byte[3];
            stream.Read(buffer, 0, 3);

            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                return Encoding.UTF8;
            }
            else if (buffer[0] == 0xFE && buffer[1] == 0xFF && buffer[2] == 0x00)
            {
                return Encoding.BigEndianUnicode;
            }
            else if (buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x41)
            {
                return Encoding.Unicode;
            }
            else
            {
                throw new Exception("无法识别的文件编码");
            }
        }
        #endregion

        #region 日期转换相关

        /// <summary>
        /// 生成一个介于_MinSeconds和_MaxSeconds之间的随机秒数
        /// </summary>
        /// <param name="_MinSeconds">最小秒数</param>
        /// <param name="_MaxSeconds">最大秒数</param>
        /// <returns>随机秒数</returns>
        public int RandomSeconds(int _MinSeconds, int _MaxSeconds)
        {
            // 创建一个随机数生成器
            Random random = new Random();

            // 生成一个介于_MinSeconds和_MaxSeconds之间的随机数
            int randomSeconds = random.Next(_MinSeconds, (_MaxSeconds + 1));

            // 将秒转换为毫秒
            int randomIntervalMs = randomSeconds * 1000;

            return randomIntervalMs;
        }

        /// <summary>
        /// 对给定的被除数进行除法运算，并调整结果
        /// </summary>
        /// <param name="dividend">被除数</param>
        /// <param name="divisor">除数</param>
        /// <returns>调整后的结果</returns>
        public int DivideAndAdjust(int dividend, int divisor)
        {
            if (divisor == 0)
            {
                throw new ArgumentException("除数不能为0。", nameof(divisor));
            }
            int result = dividend / divisor; // 先进行整除得到商
            if (dividend % divisor != 0) // 判断是否有余数，即是否能被整除
            {
                result++; // 如果有余数，则结果加1
            }
            return result;
        }

        /// <summary>
        /// 获取从格林威治时间到当前某一时刻的总毫秒数
        /// </summary>
        /// <param name="dateTime">北京时间</param>
        /// <param name="accurateToMilliseconds">精确到毫秒，否到秒</param>
        /// <returns>返回一个长整数时间戳</returns>
        public long ConvertDateTimeToLong(DateTime dateTime, bool accurateToMilliseconds)
        {
            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1, 8, 0, 0));//北京所在东八区
            DateTime endTime = TimeZoneInfo.ConvertTimeToUtc(dateTime);
            return (long)(accurateToMilliseconds ? (endTime - startTime).TotalMilliseconds : (endTime - startTime).TotalSeconds);
        }

        /// <summary>
        /// 将UNIX时间戳转为北京时间
        /// </summary>
        /// <param name="unixTimeStamp">时间戳</param>
        /// <param name="accurateToMilliseconds">精确到毫秒,否为秒</param>
        public DateTime ConvertLongToDateTime(long unixTimeStamp, bool accurateToMilliseconds)
        {
            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1, 8, 0, 0));//北京所在东八区
            return (accurateToMilliseconds ? startTime.AddMilliseconds(unixTimeStamp) : startTime.AddSeconds(unixTimeStamp)).ToLocalTime();
        }

        #endregion 日期转换相关

    }
}
