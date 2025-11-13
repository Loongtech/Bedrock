using System.Text.RegularExpressions;

namespace Net.LoongTech.Bedrock.Core.Extensions
{
    
    /// <summary>
    /// 提供一系列针对 System.DateTime 和 long 类型的扩展方法，用于Unix时间戳转换。
    /// </summary>
    public static class DateTimeExtensions
    {
        // 定义UTC时区的1970年1月1日作为Unix时间的起点
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 将 DateTime 对象转换为 Unix 时间戳 (从1970-01-01 00:00:00 UTC开始计算)。
        /// </summary>
        /// <param name="dateTime">要转换的 DateTime 对象。会自动处理其 Kind 属性（Utc或Local）。</param>
        /// <param name="accurateToMilliseconds">如果为 true，返回毫秒级时间戳；否则返回秒级时间戳。</param>
        /// <returns>一个长整型的 Unix 时间戳。</returns>
        public static long ToUnixTimestamp(this DateTime dateTime, bool accurateToMilliseconds = false)
        {
            // 将输入时间转换为UTC时间，以保证计算的准确性
            var utcDateTime = dateTime.ToUniversalTime();

            // 计算时间差
            var timespan = utcDateTime - UnixEpoch;

            // 根据精度要求返回总毫秒数或总秒数
            return accurateToMilliseconds ? (long)timespan.TotalMilliseconds : (long)timespan.TotalSeconds;
        }

        /// <summary>
        /// 将 Unix 时间戳转换为 DateTime 对象。
        /// </summary>
        /// <param name="unixTimeStamp">长整型的 Unix 时间戳。</param>
        /// <param name="isMilliseconds">该时间戳是否为毫秒级。</param>
        /// <returns>一个表示该时间戳的 DateTime 对象，其 Kind 属性为 Utc。</returns>
        public static DateTime ToDateTimeFromUnix(this long unixTimeStamp, bool isMilliseconds = false)
        {
            // 根据时间戳的精度（秒或毫秒）添加到Unix起点时间
            return isMilliseconds
                ? UnixEpoch.AddMilliseconds(unixTimeStamp)
                : UnixEpoch.AddSeconds(unixTimeStamp);
        }
    }
}