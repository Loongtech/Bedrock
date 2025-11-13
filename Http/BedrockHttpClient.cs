using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Net.LoongTech.Bedrock.Http
{
    /// <summary>
    /// 一个功能强大且可配置的 HTTP 客户端，提供了自动重试、指数退避和 cookie 管理等功能。
    /// 此类被设计为通过依赖注入来使用。
    /// </summary>
    public class BedrockHttpClient : IBedrockHttpClient
    {
        #region 静态成员和构造函数 (处理共享资源和配置加载)

        // 核心的 HttpClient 和 Handler 保持静态，以在整个应用程序生命周期内复用，这是避免套接字耗尽的最佳实践。
        private static readonly HttpClientHandler sharedHandler;
        private static readonly HttpClient httpClient;

        // 用于存储从文件中加载的配置，同样设为静态，只需加载一次。
        private static readonly BedrockHttpClientOptions _options;

        // 用于生成随机 User-Agent 的静态 Random 实例。
        private static readonly Random _staticRandom = new Random();

        /// <summary>
        /// 静态构造函数，在类第一次被访问时自动执行一次。
        /// 负责初始化共享的 HttpClient 和从文件加载配置。
        /// </summary>
        static BedrockHttpClient()
        {
            // 1. 加载配置
            _options = LoadConfiguration();

            // 2. 初始化 HttpClientHandler
            sharedHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            // 3. 初始化 HttpClient
            httpClient = new HttpClient(sharedHandler)
            {
                Timeout = TimeSpan.FromSeconds(300)
            };

            // 4. 设置默认请求头
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", GetRandomUserAgent());
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("zh-CN,zh;q=0.8,en-US;q=0.5,en;q=0.3");
            httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");

            // 5. 设置安全协议
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        /// <summary>
        /// 从外部文件（如果存在）加载配置，如果文件不存在，则返回带有默认值的配置对象。
        /// </summary>
        private static BedrockHttpClientOptions LoadConfiguration()
        {
            const string configFileName = "Bedrock.HttpClient.json";
            string externalConfigPath = Path.Combine(AppContext.BaseDirectory, configFileName);

            // 1. 检查外部配置文件是否存在。
            if (File.Exists(externalConfigPath))
            {
                try
                {
                    // 如果存在，则读取并反序列化。
                    string jsonContent = File.ReadAllText(externalConfigPath);
                    // 如果文件内容为空，JsonSerializer会返回null，我们需要处理这种情况。
                    var loadedOptions = JsonSerializer.Deserialize<BedrockHttpClientOptions>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // 如果成功加载了配置（即使是空文件产生的null），也返回它，否则继续执行到默认配置。
                    if (loadedOptions != null)
                    {
                        return loadedOptions;
                    }
                }
                catch (Exception){}
            }

            // 2. 如果文件不存在，或者加载失败，则直接返回一个带默认值的新实例。
            // 这些默认值是在 BedrockHttpClientOptions 类中定义的。
            return new BedrockHttpClientOptions();
        }

        #endregion

        #region 实例成员和构造函数 (处理每次请求的状态)

        // 每个 BedrockHttpClient 实例独有的日志记录器和随机数生成器。
        private readonly ILogger _logger;
        private readonly Random _instanceRandom = new Random();

        /// <summary>
        /// 初始化 BedrockHttpClient 的一个新实例。
        /// </summary>
        /// <param name="logger">由依赖注入容器提供的日志记录器。</param>
        public BedrockHttpClient(ILogger<BedrockHttpClient> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region 核心私有方法

        /// <summary>
        /// 获取随机User-Agent字符串，用于模拟不同浏览器访问。
        /// </summary>
        private static string GetRandomUserAgent()
        {
            string[] userAgents = {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:107.0) Gecko/20100101 Firefox/107.0",
                "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36",
                "Mozilla/5.0 (X11; Linux x86_64; rv:102.0) Gecko/20100101 Firefox/102.0"
            };
            return userAgents[_staticRandom.Next(userAgents.Length)];
        }

        /// <summary>
        /// 带重试机制的HTTP请求执行方法。它现在使用从配置文件加载的设置。
        /// </summary>
        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string url)
        {
            int retryAttempt = 0;

            while (true)
            {
                try
                {
                    // 尝试执行操作，如果成功则直接返回。
                    return await operation();
                }
                catch (Exception ex)
                {
                    retryAttempt++;
                    // 如果重试次数超过了配置的最大值，则记录最终错误并抛出异常。
                    if (retryAttempt > _options.MaxRetries)
                    {
                        _logger.LogError(ex, "请求在经过 {MaxRetries} 次重试后最终失败。URL: {Url}", _options.MaxRetries, url);
                        throw new Exception($"请求在经过 {_options.MaxRetries} 次重试后仍失败。URL -> {url}", ex);
                    }

                    // 实现指数退避算法。
                    double backoffSeconds = _options.InitialRetryDelaySeconds * Math.Pow(2, retryAttempt - 1);
                    var delay = TimeSpan.FromSeconds(backoffSeconds);

                    // 增加抖动（Jitter），防止多个实例在同一时间重试。
                    var jitter = TimeSpan.FromSeconds(_instanceRandom.Next(0, _options.JitterMaxSeconds));
                    var totalDelay = delay.Add(jitter);

                    // 使用注入的 ILogger 记录警告信息，提供结构化日志。
                    _logger.LogWarning(ex,
                        "请求失败，将在 {DelaySeconds:F2} 秒后进行第 {Attempt}/{MaxRetries} 次重试。URL: {Url}",
                        totalDelay.TotalSeconds, retryAttempt, _options.MaxRetries, url);

                    await Task.Delay(totalDelay);
                }
            }
        }

        /// <summary>
        /// 读取HTTP响应内容，自动处理GZIP解压缩。
        /// </summary>
        private async Task<string> ReadContentAsync(HttpResponseMessage response, Encoding encoding)
        {
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                if (response.Content.Headers.ContentEncoding.Any(e => e.Equals("gzip", StringComparison.InvariantCultureIgnoreCase)))
                {
                    using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(decompressedStream, encoding))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                else
                {
                    using (var reader = new StreamReader(responseStream, encoding))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
        }

        #endregion

        #region 公共方法实现 (实现 IBedrockHttpClient 接口)

        /// <summary>
        /// 获取指定URL的Cookie信息
        /// </summary>
        /// <param name="url">目标URL</param>
        /// <param name="headers">可选的自定义请求头</param>
        /// <returns>Cookie键值对列表</returns>
        public async Task<List<KeyValuePair<string, string>>> GetCookiesAsync(string url, Dictionary<string, string>? headers = null)
        {
            // MODIFICATION: 移除了多余的 HttpClientHandler 和 HttpClient 实例创建，复用全局静态实例。
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                using (var response = await httpClient.SendAsync(requestMessage))
                {
                    response.EnsureSuccessStatusCode();

                    var cookies = response.Headers.GetValues("Set-Cookie");
                    var cookieList = new List<KeyValuePair<string, string>>();

                    foreach (var cookie in cookies)
                    {
                        var cookieParts = cookie.Split(';');
                        var keyValue = cookieParts[0].Split('=');
                        if (keyValue.Length == 2)
                        {
                            cookieList.Add(new KeyValuePair<string, string>(keyValue[0], keyValue[1]));
                        }
                    }
                    return cookieList;
                }
            }
        }

        /// <summary>
        /// 发送HTTP GET请求
        /// </summary>
        /// <param name="url">请求URL</param>
        /// <param name="referer">Referer头信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="encoding">响应内容编码</param>
        /// <param name="cookies">Cookie信息</param>
        /// <returns>响应内容</returns>
        public async Task<string> HttpGet(string url, string? referer = null, Dictionary<string, string>? headers = null, Encoding? encoding = null, List<KeyValuePair<string, string>>? cookies = null)
        {
            var responseEncoding = encoding ?? Encoding.UTF8;
            return await ExecuteWithRetryAsync(async () =>
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    if (!string.IsNullOrWhiteSpace(referer))
                        requestMessage.Headers.Referrer = new Uri(referer);

                    if (headers != null)
                    {
                        foreach (var header in headers)
                            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }

                    if (cookies != null && cookies.Count > 0)
                    {
                        var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));
                        requestMessage.Headers.Add("Cookie", cookieHeader);
                    }

                    using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage))
                    {
                        response.EnsureSuccessStatusCode();
                        return await ReadContentAsync(response, responseEncoding);
                    }
                }
            }, url);
        }
        /// <summary>
        /// 发送HTTP POST请求（参数为Dictionary）
        /// </summary>
        /// <param name="jobName">任务名称，用于日志记录</param>
        /// <param name="requestUrl">请求URL</param>
        /// <param name="referer">Referer头信息</param>
        /// <param name="parameters">POST参数</param>
        /// <param name="cookies">Cookie信息</param>
        /// <returns>响应内容</returns>
        public async Task<string> HttpPost(string requestUrl, string referer, Dictionary<string, string> parameters, List<KeyValuePair<string, string>>? cookies = null)
        {
            return await ExecuteWithRetryAsync(
                async () => {
                    // MODIFICATION: 使用 HttpRequestMessage
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                    {
                        if (!string.IsNullOrWhiteSpace(referer))
                            requestMessage.Headers.Referrer = new Uri(referer);

                        if (cookies != null && cookies.Count > 0)
                        {
                            var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));
                            requestMessage.Headers.Add("Cookie", cookieHeader);
                        }

                        requestMessage.Content = new FormUrlEncodedContent(parameters);

                        using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage))
                        {
                            response.EnsureSuccessStatusCode();
                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                },
                requestUrl
            );
        }

        /// <summary>
        /// 发送HTTP POST请求（参数为KeyValuePair列表）
        /// </summary>
        /// <param name="requestUrl">请求URL</param>
        /// <param name="referer">Referer头信息</param>
        /// <param name="parameters">POST参数</param>
        /// <param name="cookies">Cookie信息</param>
        /// <returns>响应内容</returns>
        public async Task<string> HttpPost(string requestUrl, string referer, List<KeyValuePair<string, string>> parameters, List<KeyValuePair<string, string>>? cookies = null)
        {
            return await ExecuteWithRetryAsync(
               async () => {
                   using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                   {
                       if (!string.IsNullOrWhiteSpace(referer))
                           requestMessage.Headers.Referrer = new Uri(referer);

                       if (cookies != null && cookies.Count > 0)
                       {
                           var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));
                           requestMessage.Headers.Add("Cookie", cookieHeader);
                       }

                       requestMessage.Content = new FormUrlEncodedContent(parameters);

                       using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage))
                       {
                           response.EnsureSuccessStatusCode();
                           return await response.Content.ReadAsStringAsync();
                       }
                   }
               },
               requestUrl
           );
        }

        /// <summary>
        /// 发送HTTP POST请求（带自定义请求头）
        /// </summary>
        /// <param name="requestUrl">请求URL</param>
        /// <param name="referer">Referer头信息</param>
        /// <param name="headers">自定义请求头</param>
        /// <param name="parameters">POST参数</param>
        /// <param name="cookies">Cookie信息</param>
        /// <returns>响应内容</returns>
        public async Task<string> HttpPost(string requestUrl, string referer, Dictionary<string, string> headers, Dictionary<string, string> parameters, List<KeyValuePair<string, string>>? cookies = null)
        {
            return await ExecuteWithRetryAsync(                 
                async () => {
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                    {
                        if (!string.IsNullOrWhiteSpace(referer))
                            requestMessage.Headers.Referrer = new Uri(referer);

                        if (headers != null)
                        {
                            foreach (var header in headers)
                                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        if (cookies != null && cookies.Count > 0)
                        {
                            var cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Key}={c.Value}"));
                            requestMessage.Headers.Add("Cookie", cookieHeader);
                        }

                        requestMessage.Content = new FormUrlEncodedContent(parameters);

                        using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage))
                        {
                            response.EnsureSuccessStatusCode();
                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                },
                requestUrl
            );
        }

        /// <summary>
        /// 发送HTTP POST请求（JSON或表单数据）
        /// </summary>
        /// <param name="requestUrl">请求URL</param>
        /// <param name="referer">Referer头信息</param>
        /// <param name="jsonParams">POST数据</param>
        /// <param name="isJson">是否为JSON数据</param>
        /// <returns>响应内容</returns>
        public async Task<string> HttpPost(string requestUrl, string referer, string jsonParams, bool isJson = false)
        {
            return await ExecuteWithRetryAsync(
                async () => {
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                    {
                        if (!string.IsNullOrWhiteSpace(referer))
                            requestMessage.Headers.Referrer = new Uri(referer);

                        string mediaType = isJson ? "application/json" : "application/x-www-form-urlencoded";
                        requestMessage.Content = new StringContent(jsonParams, Encoding.UTF8, mediaType);

                        using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage))
                        {
                            response.EnsureSuccessStatusCode();
                            return await ReadContentAsync(response, Encoding.UTF8);
                        }
                    }
                },
                requestUrl
            );
        }

        /// <summary>
        /// 发送HTTP POST请求（表单数据）
        /// </summary>
        /// <param name="requestUrl">请求URL</param>
        /// <param name="jsonParams">POST数据</param>
        /// <returns>响应内容</returns>
        public async Task<string> HttpPost(string requestUrl, string jsonParams)
        {
            return await ExecuteWithRetryAsync(
                async () =>
                {
                    using (var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl))
                    {
                        requestMessage.Content = new StringContent(jsonParams, Encoding.UTF8, "application/x-www-form-urlencoded");
                        using (HttpResponseMessage response = await httpClient.SendAsync(requestMessage))
                        {
                            response.EnsureSuccessStatusCode();
                            return await ReadContentAsync(response, Encoding.UTF8);
                        }
                    }
                },
                requestUrl
            );
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="requestUrl">文件URL</param>
        /// <param name="fileName">保存的文件名</param>
        /// <param name="filePath">保存路径</param>
        /// <returns>文件完整路径</returns>
        public async Task<string> HttpDownFile(string requestUrl, string fileName, string filePath)
        {
            try
            {
                // REASON: 复用静态 httpClient 实例
                using (HttpResponseMessage response = await httpClient.GetAsync(requestUrl))
                {
                    response.EnsureSuccessStatusCode();

                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    string localFile = Path.Combine(filePath, fileName);

                    if (File.Exists(localFile))
                    {
                        return localFile;
                    }

                    await using (var fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }

                    return File.Exists(localFile) ? localFile : string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"根据URL -> {requestUrl} 下载附件时出错 -> {ex.Message}", true);
                return string.Empty;
            }
        }
        #endregion
    }
}