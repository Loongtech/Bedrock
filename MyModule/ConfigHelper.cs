using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Net.LoongTech.OmniCoreX
{
    /// <summary>
    /// 主程序 配置文件读取类
    /// </summary>
    public class ConfigHelper
    {
        static IConfiguration Configuration { get; set; }
        static ConfigHelper()
        {
            //ReloadOnChange = true 当appsettings.json被修改时重新加载            
            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .Add(new JsonConfigurationSource
                {
                    Path = "appsettings.json",
                    ReloadOnChange = true   //当appsettings.json被修改时重新加载    
                })
                .Build();
        }
        /// <summary>
        /// 获取 Oracle 连接字符串
        /// </summary>
        public string OracleConnString
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Configuration.GetConnectionString("Oracle11G")))
                    return AesHelper.Decrypt(Configuration.GetConnectionString("Oracle11G"));
                else
                    return string.Empty;
            }
        }



        /// <summary>
        /// 获取 任务DLL 文件所在目录名称
        /// </summary>
        public string JobFilePath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Configuration["JobFilePath"].ToString()))
                    return AppContext.BaseDirectory + Configuration["JobFilePath"].ToString();
                else
                    return AppContext.BaseDirectory;
            }
        }

        /// <summary>
        /// 获取 界面上的操作日志的最大显示行数(仅适用WINDOWS客户端)
        /// </summary>
        public int LogMaxLine
        {
            get
            {
                int renturnValue = 500;
                int.TryParse(Configuration["LogLine"].ToString(), out renturnValue);
                return renturnValue;
            }
        }

        /// <summary>
        /// 获取 重试次数
        /// </summary>
        public int MaxRetries
        {
            get
            {
                int renturnValue = 5;
                int.TryParse(Configuration["MaxRetries"].ToString(), out renturnValue);
                return renturnValue;
            }
        }

        /// <summary>
        /// 获取 重试间隔时间,单位分钟
        /// </summary>
        public int RetryInterval
        {
            get
            {
                int renturnValue = 3;
                int.TryParse(Configuration["RetryInterval"].ToString(), out renturnValue);
                return renturnValue;
            }
        }

    }
}
