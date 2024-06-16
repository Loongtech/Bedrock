using HtmlAgilityPack;
using System.Data;
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

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            handler.UseCookies = true; // 确保使用Cookie容器

            HttpClient client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537");
            //忽略SSL证书错误
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            //明确设置TLS协议版本
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            return client;
        }

        private static void AddHeaders(Dictionary<string, string> _headers, string _referer)
        {
            // 清除现有的头部信息
            httpClient.DefaultRequestHeaders.Clear();

            if (!string.IsNullOrWhiteSpace(_referer))
                httpClient.DefaultRequestHeaders.Referrer = new Uri(_referer);

            if (_headers != null)
            {
                foreach (var header in _headers)
                {
                    if (!httpClient.DefaultRequestHeaders.TryGetValues(header.Key, out _)) //防止添加重复的键值
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="_jobName">任务名称</param>
        /// <param name="_url">请求的网址</param>
        /// <param name="_referer">请求的Referer</param>
        /// <param name="_header">请求的自定义头</param>
        /// <param name="_encoding">字符编码，如 Encoding.GetEncoding("GB2312")</param>
        /// <returns></returns>
        public static async Task<string> HttpGet(string _jobName, string _url, string _referer = null, Dictionary<string, string> _headers = null, Encoding _encoding = null)
        {
            ConfigHelper configHelper = new ConfigHelper();
            int maxRetries = configHelper.MaxRetries; //重试次数
            int RetryInterval = configHelper.RetryInterval;//重试间隔
            TimeSpan retryInterval = TimeSpan.FromMinutes(RetryInterval); //重试间隔
            int retryAttempt = 0;

            AddHeaders(_headers, _referer);

            while (retryAttempt <= maxRetries)
            {
                try
                {
                    // 发送 GET 请求
                    HttpResponseMessage response = await httpClient.GetAsync(_url);
                    // 确保请求成功
                    response.EnsureSuccessStatusCode();

                    // Determine the encoding (if specified, or use default)
                    Encoding responseEncoding = _encoding ?? Encoding.UTF8;
                    string responseContent = string.Empty;
                    // 检查响应头部是否包含 Content - Encoding 字段
                    if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                    {
                        // 解压缩响应内容
                        using (var decompressedStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                        using (var reader = new System.IO.StreamReader(decompressedStream, responseEncoding))
                        {
                            responseContent = reader.ReadToEnd();
                        }
                    }
                    else
                    {
                        // 读取响应内容
                        // Read response content as bytes
                        byte[] responseBytes = await response.Content.ReadAsByteArrayAsync();

                        // Decode the response content using the determined encoding
                        responseContent = responseEncoding.GetString(responseBytes);
                    }
                    return responseContent;
                }
                catch (Exception ex)
                {
                    // If an exception occurs, check if retries are allowed and wait for the configured interval before retrying.
                    if (retryAttempt < maxRetries)
                    {
                        retryAttempt++;
                        if (retryInterval != default)
                        {
                            new LogHelper().SendEvent(_jobName, $"网站 GET 请求失败,等待 {RetryInterval} 分钟重新请求 ! URL -> {_url}", true);
                            await Task.Delay(retryInterval);
                        }
                    }
                    else
                    {
                        // If all retries are exhausted, rethrow the exception.
                        throw new Exception($"经过 {maxRetries} 次重试(每次间隔 {retryInterval.TotalMinutes} 分钟),仍然无法执行网站 GET 请求 !!! URL -> {_url} ");
                    }
                }

            }
            return null; // 这永远不应该达到;只是为了完整性而添加的。
        }

        ///  <summary>
        /// Post请求发送
        /// </summary>
        /// <param name="_jobName">任务名称</param>
        /// <param name="_url">请求的网址</param>
        /// <param name="_referer">请求的Referer</param>
        /// <param name="_header">请求的自定义头</param>
        /// <param name="postParams">传递参数
        /// Dictionary<string, string> postParams = 
        ///     new Dictionary<string, string>()
        ///     {
        ///         {"say","Hello" },
        ///         {"ask","question" }
        ///     };
        /// </param>
        /// <returns></returns>
        public static async Task<string> HttpPost(string _jobName, string _requestUrl, string _referer, Dictionary<string, string> parameters)
        {
            ConfigHelper configHelper = new ConfigHelper();
            int maxRetries = configHelper.MaxRetries; //重试次数
            int RetryInterval = configHelper.RetryInterval;//重试间隔
            TimeSpan retryInterval = TimeSpan.FromMinutes(RetryInterval); //重试间隔
            int retryAttempt = 0;

            AddHeaders(null, _referer);
            HttpContent content = new FormUrlEncodedContent(parameters);

            while (retryAttempt <= maxRetries)
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(_requestUrl, content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    // If an exception occurs, check if retries are allowed and wait for the configured interval before retrying.
                    if (retryAttempt < maxRetries)
                    {
                        retryAttempt++;
                        if (retryInterval != default)
                        {
                            new LogHelper().SendEvent(_jobName, $"网站 POST 请求失败,等待 {RetryInterval} 分钟重新请求 ! URL -> {_requestUrl}", true);
                            await Task.Delay(retryInterval);
                        }
                    }
                    else
                    {
                        // If all retries are exhausted, rethrow the exception.
                        throw new Exception($"经过 {maxRetries} 次重试(每次间隔 {retryInterval.TotalMinutes} 分钟),仍然无法执行网站 POST 请求 !!! URL -> {_requestUrl} ");
                    }
                }

            }
            return null; // 这永远不应该达到;只是为了完整性而添加的。


        }

        /// <summary>
        /// Post请求Json参数
        /// </summary>
        /// <param name="_jobName">任务名称</param>
        /// <param name="_requestUrl"></param>
        /// <param name="_jsonParams"></param>
        /// <returns></returns>
        public static async Task<string> HttpPost(string _jobName, string _requestUrl, string _referer, string _jsonParams)
        {
            ConfigHelper configHelper = new ConfigHelper();
            int maxRetries = configHelper.MaxRetries; //重试次数
            int RetryInterval = configHelper.RetryInterval;//重试间隔
            TimeSpan retryInterval = TimeSpan.FromMinutes(RetryInterval); //重试间隔
            int retryAttempt = 0;

            AddHeaders(null, _referer);
            HttpContent content = new StringContent(_jsonParams, Encoding.UTF8, "application/x-www-form-urlencoded");

            while (retryAttempt <= maxRetries)
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(_requestUrl, content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    // If an exception occurs, check if retries are allowed and wait for the configured interval before retrying.
                    if (retryAttempt < maxRetries)
                    {
                        retryAttempt++;
                        if (retryInterval != default)
                        {
                            new LogHelper().SendEvent("WebDataCapture", $"网站 POST 请求失败,等待 {RetryInterval} 分钟重新请求 ! URL -> {_requestUrl}", true);
                            await Task.Delay(retryInterval);
                        }
                    }
                    else
                    {
                        // If all retries are exhausted, rethrow the exception.
                        throw new Exception($"经过 {maxRetries} 次重试(每次间隔 {retryInterval.TotalMinutes} 分钟),仍然无法执行网站 POST 请求 !!! URL -> {_requestUrl} ");
                    }
                }

            }
            return null; // 这永远不应该达到;只是为了完整性而添加的。
        }


        /// <summary>
        /// Post请求Json参数
        /// </summary>
        /// <param name="_jobName">任务名称</param>
        /// <param name="_requestUrl"></param>
        /// <param name="_jsonParams"></param>
        /// <returns></returns>
        public static async Task<string> HttpPost(string _jobName, string _requestUrl, string _jsonParams)
        {
            ConfigHelper configHelper = new ConfigHelper();
            int maxRetries = configHelper.MaxRetries; //重试次数
            int RetryInterval = configHelper.RetryInterval;//重试间隔
            TimeSpan retryInterval = TimeSpan.FromMinutes(RetryInterval); //重试间隔
            int retryAttempt = 0;

            HttpContent content = new StringContent(_jsonParams, Encoding.UTF8, "application/x-www-form-urlencoded");

            while (retryAttempt <= maxRetries)
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync(_requestUrl, content);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception)
                {
                    // If an exception occurs, check if retries are allowed and wait for the configured interval before retrying.
                    if (retryAttempt < maxRetries)
                    {
                        retryAttempt++;
                        if (retryInterval != default)
                        {
                            new LogHelper().SendEvent(_jobName, $"网站 POST 请求失败,等待 {RetryInterval} 分钟重新请求 ! URL -> {_requestUrl}", true);
                            await Task.Delay(retryInterval);
                        }
                    }
                    else
                    {
                        // If all retries are exhausted, rethrow the exception.
                        throw new Exception($"经过 {maxRetries} 次重试(每次间隔 {retryInterval.TotalMinutes} 分钟),仍然无法执行网站 POST 请求 !!! URL -> {_requestUrl} ");
                    }
                }

            }
            return null; // 这永远不应该达到;只是为了完整性而添加的。
        }



        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="_requestUrl">下载地址</param>
        /// <param name="_referer">Referer</param>
        /// <param name="_header">Header</param>
        /// <param name="_fileName">文件名</param>
        /// <param name="_filePath">文件路径</param>
        /// <returns></returns>
        public static async Task<string> HttpDownFile(string _jobName, string _requestUrl, string _fileName, string _filePath)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();
                // Ignore SSL certificate errors
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                httpClient.Timeout = new TimeSpan(0, 0, 0, 60);//超时60秒
                // Set TLS protocol version explicitly
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;


                HttpResponseMessage response = await httpClient.GetAsync(_requestUrl);
                response.EnsureSuccessStatusCode();

                //判断指定的目录是否存在
                if (!Directory.Exists(_filePath))
                    Directory.CreateDirectory(_filePath);//如果不存就创建


                // 将文件内容保存到本地
                string localFile = Path.Combine(_filePath, _fileName);
                await using var fileStream = File.Create(localFile);
                await response.Content.CopyToAsync(fileStream);

                if (File.Exists(localFile))
                    return localFile;
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                new LogHelper().SendEvent(_jobName, $"根据URL -> {_requestUrl} 下载附件时出错 -> {ex.Message}", true); //发送消息通知        
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
        /// <param name="_unicodeString">Unicode 字符串</param>
        /// <returns></returns>
        public static string DecodeUnicodeString(string _unicodeString)
        {

            string[] unicodeSegments = _unicodeString.Split(new[] { "\\u" }, StringSplitOptions.RemoveEmptyEntries);
            var builder = new StringBuilder();

            foreach (string segment in unicodeSegments)
            {
                int codePoint = int.Parse(segment, System.Globalization.NumberStyles.HexNumber);
                builder.Append(char.ConvertFromUtf32(codePoint));
            }

            return builder.ToString();
        }

        /// <summary>
        /// 将 HTML 中的表格转换为 DataTable
        /// </summary>
        /// <param name="_htmlTable"></param>
        /// <returns></returns>
        public static DataTable HtmlTableToDataTable(HtmlNode _htmlTable)
        {
            DataTable returnValue = new DataTable();
            bool isHead = true;//是否是列头
            foreach (HtmlNode row in _htmlTable.SelectNodes(@".//tr"))
            {
                if (isHead)
                {
                    foreach (HtmlNode cell in row.SelectNodes(@".//th|.//td"))
                    {
                        DataColumn dc1 = new DataColumn(HttpHelper.ReplaceHtmlTag(cell.InnerText));
                        returnValue.Columns.Add(dc1);
                    }
                    isHead = false;
                }
                else
                {
                    DataRow dr = returnValue.NewRow();
                    int i = 0;
                    foreach (HtmlNode cell in row.SelectNodes(@".//th|.//td"))
                    {
                        dr[i] = HttpHelper.ReplaceHtmlTag(cell.InnerText);
                        i++;
                    }
                    returnValue.Rows.Add(dr);
                }
            }

            return returnValue;
        }
    }
}
