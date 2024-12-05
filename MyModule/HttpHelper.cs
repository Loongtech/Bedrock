using System.IO.Compression;
using System.Net;
using System.Text;

namespace Net.LoongTech.OmniCoreX
{
    /// <summary>
    /// http 请求相关
    /// </summary>
    public static class HttpHelper
    {

        private static readonly HttpClient httpClient = CreateHttpClient();

        /// <summary>
        /// 创建并配置一个 HttpClient 实例。
        /// </summary>
        /// <returns>配置好的 HttpClient 实例。</returns>
        private static HttpClient CreateHttpClient()
        {
            // 创建一个新的 HttpClient 实例
            HttpClient client = new HttpClient();

            // 设置超时时间为 300 秒
            client.Timeout = TimeSpan.FromSeconds(300);

            // 忽略 SSL 证书错误（不建议在生产环境中使用）
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            // 明确设置 TLS 协议版本
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            // 返回配置好的 HttpClient 实例
            return client;
        }

        /// <summary>
        /// 向 HttpClient 添加指定的请求头。
        /// </summary>
        /// <param name="headers">包含请求头键值对的字典。</param>
        /// <param name="referer">可选的 Referer 地址。</param>
        private static void AddHeaders(Dictionary<string, string> headers, string referer)
        {
            // 清除现有的头部信息
            httpClient.DefaultRequestHeaders.Clear();

            // 添加 User-Agent 请求头，模拟浏览器访问
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");

            // 如果 Referer 不为空，则设置 Referer 头
            if (!string.IsNullOrWhiteSpace(referer))
                httpClient.DefaultRequestHeaders.Referrer = new Uri(referer);

            // 如果传入的头部信息字典不为空，则逐个添加头部信息
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    // 防止添加重复的键值
                    if (!httpClient.DefaultRequestHeaders.TryGetValues(header.Key, out _))
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 异步执行带有重试机制的操作。
        /// </summary>
        /// <param name="operation">要执行的异步操作。</param>
        /// <param name="url">请求的URL。</param>
        /// <exception cref="Exception">如果经过所有重试后仍然无法执行请求，将抛出异常。</exception>
        private static async Task<string> ExecuteWithRetryAsync(Func<Task<string>> operation, string url)
        {
            // 创建配置帮助器实例
            ConfigHelper configHelper = new ConfigHelper();

            // 获取重试间隔（分钟）
            int RetryInterval = configHelper.RetryInterval;
            // 将重试间隔转换为 TimeSpan 对象
            TimeSpan retryInterval = TimeSpan.FromMinutes(RetryInterval);

            // 获取最大重试次数
            int maxRetries = configHelper.MaxRetries;

            // 初始化重试次数为 0
            int retryAttempt = 0;

            // 当重试次数小于等于最大重试次数时，继续尝试
            while (retryAttempt <= maxRetries)
            {
                try
                {
                    // 尝试执行操作并返回结果
                    return await operation();
                }
                catch (Exception ex)
                {
                    if (retryAttempt < maxRetries)
                    {
                        // 如果重试次数未达到最大重试次数
                        retryAttempt++;
                        // 发送事件记录请求失败并等待重试间隔时间                        
                        new LogHelper().SendEvent("OmniCoreX", $"网站请求失败, 等待 {retryInterval.TotalMinutes} 分钟重新请求! URL -> {url}", true);
                        await Task.Delay(retryInterval);
                    }
                    else
                    {
                        // 如果经过所有重试后仍然失败，抛出异常
                        throw new Exception($"经过 {maxRetries} 次重试后，仍然无法执行请求 !!! URL -> {url}", ex);
                    }
                }
            }

            // 默认返回值; 理论上不会到达此行
            return null;
        }

        /// <summary>
        /// 异步读取 HTTP 响应的内容，并根据内容编码进行解压和解码。
        /// </summary>
        /// <param name="response">HTTP 响应对象。</param>
        /// <param name="encoding">字符编码。</param>
        /// <returns>解码后的响应内容字符串。</returns>
        private static async Task<string> ReadContentAsync(HttpResponseMessage response, Encoding encoding)
        {
            // 从响应内容中异步读取流
            Stream responseStream = await response.Content.ReadAsStreamAsync();

            // 检查响应内容是否使用了 gzip 压缩
            if (response.Content.Headers.ContentEncoding.Contains("gzip"))
            {
                // 使用 GZipStream 解压响应流
                using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                {
                    // 使用指定的编码创建 StreamReader 并读取解压后的流内容
                    using (var reader = new StreamReader(decompressedStream, encoding))
                    {
                        // 异步读取解压后的流内容并返回
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            else
            {
                // 如果响应内容没有压缩，直接读取响应内容的字节数组
                byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();
                // 使用指定的编码将字节数组转换为字符串并返回
                return encoding.GetString(responseBytes);
            }
        }

        /// <summary>
        /// 获取网站的 Cookies。
        /// </summary>
        /// <returns>返回 Cookies 列表。</returns>
        public static async Task<List<KeyValuePair<string, string>>> GetCookiesAsync(string url, Dictionary<string, string> headers = null)
        {
            if (headers != null)
                AddHeaders(headers, null);

            var handler = new HttpClientHandler() { UseCookies = true };
            using (var response = await httpClient.GetAsync(url))
            {
                response.EnsureSuccessStatusCode();

                // 从响应中提取 Cookies
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


        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="url">请求的网址</param>
        /// <param name="referer">请求的Referer</param>
        /// <param name="headers">请求的自定义头</param>
        /// <param name="encoding">字符编码，如 Encoding.GetEncoding("GB2312")</param>
        /// <param name="cookies">cookie</param>
        /// <returns></returns>
        public static async Task<string> HttpGet(string url, string referer = null, Dictionary<string, string> headers = null, Encoding encoding = null, List<KeyValuePair<string, string>> cookies = null)
        {
            AddHeaders(headers, referer);

            // 如果有 Cookies，添加到请求头
            if (cookies != null)
            {
                var cookieHeader = new StringBuilder();
                foreach (var cookie in cookies)
                {
                    cookieHeader.Append($"{cookie.Key}={cookie.Value}; ");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader.ToString().TrimEnd(' ', ';'));
            }

            Encoding responseEncoding = encoding ?? Encoding.UTF8;

            return await ExecuteWithRetryAsync(async () =>
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(url))
                {
                    response.EnsureSuccessStatusCode();
                    return await ReadContentAsync(response, responseEncoding);
                }
            }, url);
        }

        ///  <summary>
        /// Post请求发送
        /// </summary>
        /// <param name="requestUrl">请求的网址</param>
        /// <param name="referer">请求的Referer</param>
        /// <param name="parameters">传递参数
        /// Dictionary<string, string> parameters = 
        ///     new Dictionary<string, string>()
        ///     {
        ///         {"say","Hello" },
        ///         {"ask","question" }
        ///     };
        /// </param>
        /// <param name="cookies">cookies</param>
        /// <returns></returns>
        public static async Task<string> HttpPost(string requestUrl, string referer, Dictionary<string, string> parameters, List<KeyValuePair<string, string>> cookies = null)
        {
            AddHeaders(null, referer);
            HttpContent content = new FormUrlEncodedContent(parameters);

            // 如果有 Cookies，添加到请求头
            if (cookies != null)
            {
                var cookieHeader = new StringBuilder();
                foreach (var cookie in cookies)
                {
                    cookieHeader.Append($"{cookie.Key}={cookie.Value}; ");
                }
                httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader.ToString().TrimEnd(' ', ';'));
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content))
                {
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
            }, requestUrl);

        }

        /// <summary>
        /// Post请求Json参数
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="jsonParams"></param>
        /// <returns></returns>
        public static async Task<string> HttpPost(string requestUrl, string referer, string jsonParams, bool isJson = false)
        {
            AddHeaders(null, referer);

            HttpContent content;
            if (isJson)
            {
                content = new StringContent(jsonParams, Encoding.UTF8, "application/json");
            }
            else
            {
                content = new StringContent(jsonParams, Encoding.UTF8, "application/x-www-form-urlencoded");
            }

            return await ExecuteWithRetryAsync(async () =>
            {
                using (HttpResponseMessage response = await httpClient.PostAsync(requestUrl, content))
                {
                    response.EnsureSuccessStatusCode();
                    return await ReadContentAsync(response, Encoding.UTF8); // 返回 Task<string> 类型
                }
            }, requestUrl);
        }

        /// <summary>
        /// Post请求Json参数
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <param name="jsonParams"></param>
        /// <returns></returns>
        public static async Task<string> HttpPost(string requestUrl, string jsonParams)
        {
            HttpContent content = new StringContent(jsonParams, Encoding.UTF8, "application/x-www-form-urlencoded");

            return await ExecuteWithRetryAsync(async () =>
            {
                using (HttpResponseMessage response = await httpClient.GetAsync(requestUrl))
                {
                    response.EnsureSuccessStatusCode();
                    return await ReadContentAsync(response, Encoding.UTF8); // 返回 Task<string> 类型
                }
            }, requestUrl);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="requestUrl">下载地址</param>
        /// <param name="_referer">Referer</param>
        /// <param name="_header">Header</param>
        /// <param name="fileName">文件名</param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static async Task<string> HttpDownFile(string requestUrl, string fileName, string filePath)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                // Ignore SSL certificate errors
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                httpClient.Timeout = new TimeSpan(0, 0, 0, 300);//超时300秒
                // Set TLS protocol version explicitly
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

                HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                //判断指定的目录是否存在
                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);//如果不存就创建


                // 将文件内容保存到本地
                string localFile = Path.Combine(filePath, fileName);
                await using var fileStream = File.Create(localFile);
                await response.Content.CopyToAsync(fileStream);

                if (File.Exists(localFile))
                    return localFile;
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                new LogHelper().SendEvent("OmniCoreX", $"根据URL -> {requestUrl} 下载附件时出错 -> {ex.Message}", true); //发送消息通知        
                return string.Empty;
            }
        }

        /// <summary>
        /// 去除html标签
        /// </summary>
        /// <param name="html"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ReplaceHtmlTag(string html, int length = 0)
        {
            string strText = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]+>", "");
            strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");
            strText = strText.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "");

            if (length > 0 && strText.Length > length)
                return strText.Substring(0, length);

            return strText;
        }

        /// <summary>
        /// 转换 Unicode 字符串
        /// </summary>
        /// <param name="unicodeString">Unicode 字符串</param>
        /// <returns></returns>
        public static string DecodeUnicodeString(string unicodeString)
        {

            string[] unicodeSegments = unicodeString.Split(new[] { "\\u" }, StringSplitOptions.RemoveEmptyEntries);
            var builder = new StringBuilder();

            foreach (string segment in unicodeSegments)
            {
                int codePoint = int.Parse(segment, System.Globalization.NumberStyles.HexNumber);
                builder.Append(char.ConvertFromUtf32(codePoint));
            }

            return builder.ToString();
        }
    }
}
