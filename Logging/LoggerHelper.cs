using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Net.LoongTech.Bedrock.Logging
{
    /// <summary>
    /// 日志帮助类，用于创建和管理日志记录器
    /// </summary>
    public static class LoggerHelper
    {
        /// <summary>
        /// 日志工厂实例
        /// </summary>
        private static ILoggerFactory? _loggerFactory;

        /// <summary>
        /// 初始化日志工厂
        /// </summary>
        /// <param name="configuration">配置信息</param>
        /// <remarks>
        /// 此方法配置日志系统，添加控制台输出和自定义文件日志提供程序
        /// </remarks>
        public static void InitLoggerFactory(IConfiguration configuration)
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                var loggingSection = configuration?.GetSection("Logging");
                if (loggingSection != null && loggingSection.Exists())
                {
                    builder.AddConfiguration(loggingSection);
                }

                // 添加自定义格式化器
                builder.AddConsole(options =>
                {
                    options.FormatterName = "custom";
                }).AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();

                // 添加自定义文件日志
                builder.AddProvider(new CustomFileLoggerProvider());
            });
        }

        /// <summary>
        /// 创建指定类别的日志记录器
        /// </summary>
        /// <param name="category">日志类别名称</param>
        /// <returns>指定类别的ILogger实例</returns>
        /// <exception cref="InvalidOperationException">当LoggerFactory未初始化时抛出</exception>
        public static ILogger CreateLogger(string category)
        {
            if (_loggerFactory == null)
                throw new InvalidOperationException("LoggerFactory 未初始化");

            return _loggerFactory.CreateLogger(category);
        }

        /// <summary>
        /// 创建指定类型的泛型日志记录器
        /// </summary>
        /// <typeparam name="T">日志记录器关联的类型</typeparam>
        /// <returns>指定类型的ILogger{T}实例</returns>
        /// <exception cref="InvalidOperationException">当LoggerFactory未初始化时抛出</exception>
        public static ILogger<T> CreateLogger<T>()
        {
            if (_loggerFactory == null)
                throw new InvalidOperationException("LoggerFactory 未初始化");

            return _loggerFactory.CreateLogger<T>();
        }
    }
}
