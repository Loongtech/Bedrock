using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.LoongTech.Bedrock.Http
{
    /// <summary>
    /// 为 BedrockHttpClient 提供配置选项。
    /// </summary>
    public class BedrockHttpClientOptions
    {
       
        /// <summary>
        /// 获取或设置请求失败后的最大重试次数。
        /// 默认为 3 次。
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// 获取或设置初次重试前的基础延迟时间（单位：秒）。
        /// 后续的重试将基于此值进行指数退避。
        /// 默认为 5 秒。
        /// </summary>
        public int InitialRetryDelaySeconds { get; set; } = 5;

        /// <summary>
        /// 获取或设置用于重试延迟的抖动（Jitter）的最大随机秒数。
        /// 这有助于避免多个实例在完全相同的时间点重试。
        /// 默认为 15 秒。
        /// </summary>
        public int JitterMaxSeconds { get; set; } = 15;
    }
}
