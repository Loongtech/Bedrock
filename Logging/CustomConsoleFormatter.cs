using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace Net.LoongTech.Bedrock.Logging
{
    /// <summary>
    /// 自定义控制台日志格式化器，支持 ANSI 颜色输出。
    /// 用于美化日志输出，不同日志级别显示不同颜色。
    /// </summary>
    public class CustomConsoleFormatter : ConsoleFormatter
    {
        /// <summary>
        /// 初始化 <see cref="CustomConsoleFormatter"/> 类的新实例。
        /// </summary>
        public CustomConsoleFormatter() : base("custom") { }

        /// <summary>
        /// 写入格式化后的日志到控制台。
        /// </summary>
        /// <typeparam name="TState">日志状态对象类型。</typeparam>
        /// <param name="logEntry">日志条目，包含日志级别、消息、异常等信息。</param>
        /// <param name="scopeProvider">日志作用域提供器。</param>
        /// <param name="textWriter">输出目标 <see cref="TextWriter"/>。</param>
        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider scopeProvider,
            TextWriter textWriter)
        {
            var logLevel = logEntry.LogLevel;
            var now = DateTime.Now;
            // 获取格式化后的日志消息
            var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
            if (message == null) return;

            // 根据日志级别选择 ANSI 颜色
            string colorStart = logLevel switch
            {
                LogLevel.Trace => "\x1b[37m",        // 灰色
                LogLevel.Debug => "\x1b[36m",        // 青色
                LogLevel.Information => "\x1b[32m",  // 绿色
                LogLevel.Warning => "\x1b[33m",      // 黄色
                LogLevel.Error => "\x1b[31m",        // 红色
                LogLevel.Critical => "\x1b[41;37m",  // 白底红字
                _ => "\x1b[0m"
            };

            const string colorReset = "\x1b[0m"; // ANSI 重置颜色

            var logRecord = new StringBuilder();
            // 拼接日志输出内容，包含时间、级别、消息
            logRecord.Append($"{colorStart}[{now:yyyy-MM-dd HH:mm:ss}]\t[{logEntry.Category}]\t[{logLevel}]\t\t{message}");

            // 如果有异常，追加异常信息
            if (logEntry.Exception != null)
            {
                logRecord.Append($"\t{logEntry.Exception}");
            }

            logRecord.Append(colorReset);
            textWriter.WriteLine(logRecord.ToString());
        }
    }
}
