using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.LoongTech.Bedrock.Http
{
    /// <summary>
    /// 定义了 Bedrock 框架中 HTTP 客户端的功能。
    /// 这是推荐的依赖注入和使用的类型。
    /// </summary>
    public interface IBedrockHttpClient
    {
        Task<List<KeyValuePair<string, string>>> GetCookiesAsync(string url, Dictionary<string, string>? headers = null);

        Task<string> HttpGet(string url, string? referer = null, Dictionary<string, string>? headers = null, Encoding? encoding = null, List<KeyValuePair<string, string>>? cookies = null);

        Task<string> HttpPost(string requestUrl, string referer, Dictionary<string, string> parameters, List<KeyValuePair<string, string>>? cookies = null);

        Task<string> HttpPost(string requestUrl, string referer, List<KeyValuePair<string, string>> parameters, List<KeyValuePair<string, string>>? cookies = null);

        Task<string> HttpPost(string requestUrl, string referer, Dictionary<string, string> headers, Dictionary<string, string> parameters, List<KeyValuePair<string, string>>? cookies = null);

        Task<string> HttpPost(string requestUrl, string referer, string data, bool isJson = false);

        Task<string> HttpPost(string requestUrl, string data);

        Task<string> HttpDownFile(string requestUrl, string fileName, string filePath);
    }
}
