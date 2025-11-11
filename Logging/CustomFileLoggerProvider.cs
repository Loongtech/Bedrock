using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Net.LoongTech.Bedrock.Logging
{
    /// <summary>
    /// 自定义文件日志提供程序，实现ILoggerProvider接口
    /// </summary>
    public class CustomFileLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, CustomFileLogger> _loggers = new();

        /// <summary>
        /// 创建指定类别的日志记录器
        /// </summary>
        /// <param name="categoryName">日志类别名称</param>
        /// <returns>对应类别的ILogger实例</returns>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new CustomFileLogger(name));
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose() { }
    }

    /// <summary>
    /// 自定义文件日志记录器，实现ILogger接口
    /// </summary>
    public class CustomFileLogger : ILogger
    {
        private readonly string _categoryName;
        private static readonly object _lock = new();

        /// <summary>
        /// 初始化日志记录器
        /// </summary>
        /// <param name="categoryName">日志类别名称</param>
        public CustomFileLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        /// <summary>
        /// 开始一个日志作用域
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        /// <param name="state">作用域状态</param>
        /// <returns>可释放的作用域对象</returns>
        public IDisposable? BeginScope<TState>(TState state) => null;

        /// <summary>
        /// 检查指定日志级别是否启用
        /// </summary>
        /// <param name="logLevel">日志级别</param>
        /// <returns>始终返回true，表示所有级别日志都启用</returns>
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <typeparam name="TState">状态类型</typeparam>
        /// <param name="logLevel">日志级别</param>
        /// <param name="eventId">事件ID</param>
        /// <param name="state">日志状态</param>
        /// <param name="exception">异常信息</param>
        /// <param name="formatter">日志格式化器</param>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var now = DateTime.Now;
            var dateStr = now.ToString("yyyy-MM-dd");

            var subDir = Path.Combine("Logs", _categoryName);
            Directory.CreateDirectory(subDir);

            string fileName = logLevel >= LogLevel.Error ? $"err-{dateStr}.txt" : $"log-{dateStr}.txt";
            string filePath = Path.Combine(subDir, fileName);

            var logRecord = new StringBuilder();

            logRecord.Append($"[{now:yyyy-MM-dd HH:mm:ss}]\t[{logLevel}]\t\t{formatter(state, exception)}");

            if (exception != null)
            {
                logRecord.Append($"\t{exception}");
            }

            logRecord.AppendLine();

            lock (_lock)
            {
                File.AppendAllText(filePath, logRecord.ToString());
            }
        }
    }
}
