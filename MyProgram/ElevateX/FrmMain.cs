using LitJson;
using Microsoft.Web.WebView2.Core;
using Net.LoongTech.OmniCoreX;
using System.Net;
using System.Timers;


namespace Net.LoongTech.ElevateX
{
    public partial class FrmMain : Form
    {
        //配置文件获取类
        private ConfigHelper configHelper = new();

        private System.Timers.Timer timer;
        

        public FrmMain()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {

            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.Navigate("https://www.baidu.com/");
        }

        /// <summary>
        /// 当按钮1被点击时的事件处理程序。
        /// </summary>
        /// <param name="sender">事件的发送者，通常是按钮1。</param>
        /// <param name="e">事件的参数，包含有关事件的详细信息。</param>
        private void button1_Click(object sender, EventArgs e)
        {
            // 检查文本框1中的文本是否为空或只包含空白字符
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {
                // 构建百度搜索的URL，包含搜索关键字和其他参数
               
                //string url =
                //    @"https://www.baidu.com/s?" +
                //    @"ie=utf-8&f=8&rsv_bp=1&tn=baidu&rsv_pq=dac3e43a000957e6&rsv_t=8db1tuoOF1r1dy50%2BjHlWJxmjiif5PSgr8XvoKt3S8UZxK9wC0Bn8pmahPk&rqlang=cn&rsv_dl=tb&rsv_enter=1&rsv_sug3=19&rsv_sug1=1&rsv_sug7=100&rsv_sug2=0&rsv_btype=t&inputT=6393&rsv_sug4=6707" +
                //    $"&wd={textBox1.Text}&oq={textBox1.Text}";
                string url =
                    @"https://www.baidu.com/s?" +
                    @"ie=utf-8&newi=1&mod=1&isbd=1&isid=a6a049d10000bb65&rsv_spt=1&rsv_iqid=0x9ac4b6ec002dd2e8&issp=1&f=8&rsv_bp=1&rsv_idx=2&ie=utf-8&rqlang=cn&tn=baiduhome_pg&rsv_dl=tb&rsv_enter=0&oq=%E6%B5%B7%E9%A1%BA%E6%8A%95%E9%A1%BE&rsv_btype=t&rsv_t=7620ghzUKl0XzEC4EH0j+lUy5c3OeWwo43f0bQdyqB2YqZwagXV+okMBYlKcHkjktYbG&rsv_pq=a6a049d10000bb65&rsv_sid=60271_60338_60332_60346_60359_60376&_ss=1&clist=&hsug=&f4s=1&csor=4&_cr1=38493" +
                    $"&wd={textBox1.Text}&bs={textBox1.Text}";


                // 在webView21中导航到构建的URL
                webView21.CoreWebView2.Navigate(url);

                // 注册webView21的NavigationCompleted事件处理程序
                webView21.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
            }
        }

        /// <summary>
        /// 当WebView导航完成时触发的事件处理程序。
        /// </summary>
        /// <param name="sender">事件的源。</param>
        /// <param name="e">导航完成事件的参数，包含导航状态等信息。</param>
        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // 检查当前WebView的导航地址是否为百度搜索结果页面
            if (webView21.CoreWebView2.Source.Contains("www.baidu.com/s"))
            {
                // 定义JavaScript脚本，用于从网页中提取搜索结果的标题和URL
                // 提取所有搜索结果的标题和URL
                string script =

                    @"var results = Array.from(document.querySelectorAll('h3 a')).map(a => {
                     return {
                         title: a.innerText.trim(), // 使用trim()去除可能的前后空格
                         url: a.href
                     };
                 });
                 JSON.stringify(results);
                ";

                // 在WebView中执行JavaScript脚本，获取搜索结果
                string result = await webView21.CoreWebView2.ExecuteScriptAsync(script);

                // 处理JavaScript执行结果，将JSON字符串转换为C#可用的格式
                result = result.Replace("\\\"", "'").Replace("\"", "");

                try
                {
                    // 使用JsonMapper将处理后的JSON字符串转换为JsonData对象
                    JsonData jsonData = JsonMapper.ToObject(result);

                    // 清空列表视图中的项，并准备批量添加搜索结果
                    listViewEx1.Items.Clear();
                    listViewEx1.BeginUpdate();

                    // 遍历JsonData对象，将搜索结果添加到列表视图中
                    foreach (JsonData item in jsonData)
                    {
                        listViewEx1.Items.Add(new ListViewItem(new[] { item["title"].ToString(), item["url"].ToString() }));
                    }

                    // 结束列表视图的更新，确保界面刷新
                    listViewEx1.EndUpdate();
                }
                catch (JsonException jsonEx)
                {
                    // 如果JSON解析出错，通过SendEvent发送错误事件
                    SendEvent("ElevateX", $"JSON解析错误: {jsonEx.Message}", true);
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedItems.Count < 1)
                MessageBox.Show("请从右边的列表中至少选择一个项目");
            else
            {
                if (btnRun.Text == "开始")
                {
                    InitializeTimer();

                    btnRun.Text = "停止";
                    SendEvent("ElevateX", $"开始，每隔 {numRunTime.Value} 分钟自动运行", false); //在屏幕上打印日志
                }
                else
                {
                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    SendEvent("ElevateX", "停止自动运行", false); //在屏幕上打印日志
                    btnRun.Text = "开始";
                }
            }                
        }

        /// <summary>
        /// 获取代理列表
        /// </summary>
        /// <returns></returns>
        private List<ProxyInfo> GetProxyList()
        {
            // 定义一个返回值，用于存储代理信息
            List<ProxyInfo> returnValue = new List<ProxyInfo>();
            try
            {
                // 从配置文件中获取代理列表的地址
                string Url_Proxy = new ConfigHelper().GetProxyList;
                // 如果地址为空，则提示用户
                if (string.IsNullOrEmpty(Url_Proxy))
                {
                    MessageBox.Show("未配置代理列表");
                    return null;
                }
                else
                {
                    // 使用HttpClient请求代理列表
                    using var httpClient = new HttpClient();

                    var response = httpClient.GetAsync(Url_Proxy).Result;

                    // 如果请求成功，则解析返回的Json数据
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;

                        JsonData jsonData = JsonMapper.ToObject(content);

                        if (jsonData["code"].ToString() != "0")
                        {
                            throw new Exception(jsonData["msg"].ToString());
                        }
                        else
                        {
                            // 遍历Json数据，将代理信息添加到返回值中
                            foreach (JsonData item in jsonData["data"])
                            {
                                returnValue.Add(
                                    new ProxyInfo()
                                    {
                                        Host = item["ip"].ToString(),
                                        Port = int.Parse(item["port"].ToString()),
                                        ExpireTime = DateTime.TryParse(item["expire_time"].ToString(), out DateTime expireTime) ? expireTime : DateTime.Now.AddMinutes(2)
                                    }
                                );
                            }
                        }
                       
                    }
                    // 如果请求失败，则抛出异常
                    else
                    {
                        throw new Exception(response.StatusCode.ToString());
                    }
                }
            }
            // 如果发生异常，则发送错误信息
            catch (Exception ex)
            {
                SendEvent("ElevateX", $"【失败】[GetProxyList] 错误 {ex.Message}", true);
            }
            // 返回代理信息
            return returnValue;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {

            numRunTime.Value = configHelper.RunTime;
            //订阅事件
            AlarmHelper alarm = AlarmHelper.Instance;
            alarm.AlarmEvent += new AlarmHelper.AlarmEventHandler(JobAlarmEvent);
        }
                

        /// <summary>
        /// 使用指定的代理服务器访问指定的网站
        /// </summary>
        /// <param name="urlWebSite"></param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        private async ValueTask VisitWebSite(string urlWebSite, string titleWeb, ProxyInfo proxy)
        {
             
            // 定义目标网站地址
            string targetUrl = urlWebSite;
            // 定义代理服务器地址
            string proxyAddress = $"http://{proxy.Host}:{proxy.Port}";

            // 创建HttpClientHandler并设置代理
            var proxySrv = new WebProxy(proxyAddress);
            HttpClientHandler httpClientHandler = new ()
            {
                Proxy = proxySrv
            };
            
            // 创建HttpClient实例
            using var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Add("Header-Key", "header-vaule");
            //Android 系统下的微信小程序的 User-Agent
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 7.1.1; MI 6 Build/NMF26X; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/57.0.2987.132 MQQBrowser/6.2 TBS/043807 Mobile Safari/537.36 MicroMessenger/6.6.1.1220(0x26060135) NetType/4G Language/zh_CN MicroMessenger/6.6.1.1220(0x26060135) NetType/4G Language/zh_CN miniProgram");
             

            try
            {                
                // 发送GET请求
                HttpResponseMessage response = await httpClient.GetAsync(targetUrl) ;
               
                if (response.IsSuccessStatusCode)
                {
                    // 如果请求成功，读取响应内容
                    //string content = response.Content.ReadAsStringAsync().Result;
                    SendEvent("ElevateX", $"【成功】通过 {proxyAddress} 访问 【{titleWeb}】-> {urlWebSite} 成功，状态码: {(int)response.StatusCode} - {response.ReasonPhrase}", false);
                }
                else
                {
                    // 如果请求失败，发送事件
                    SendEvent("ElevateX", $"【失败】通过 {proxyAddress} 访问 【{titleWeb}】-> {urlWebSite} 失败，状态码: {(int)response.StatusCode} - {response.ReasonPhrase}", true);
                }
            }
            catch (Exception ex)
            {
                // 如果发生异常，发送事件
                SendEvent("ElevateX", $"【失败】通过 {proxyAddress} 访问 【{titleWeb}】-> 过程中发生错误: {ex.Message} ", true);
            }
        }

        /// <summary>
        /// 消息发送事件
        /// </summary>
        /// <param name="_alermName">消息的名称</param>
        /// <param name="_alermMsg">消息的内容</param>
        /// <param name="_alermLevel">消息内容是否是错误</param>
        private void SendEvent(string _alermName, string _alermMsg, bool _alermIsErr)
        {
            AlarmHelper.AlermEventArgs e = new AlarmHelper.AlermEventArgs(_alermName, _alermMsg, _alermIsErr);
            AlarmHelper.Instance.SendEvent(e);
        }

        /// <summary>
        /// 消息发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void JobAlarmEvent(object sender, AlarmHelper.AlermEventArgs eventArgs)
        {
            this.BeginInvoke(new AlarmHelper.AlarmEventHandler(WriteScreenLog), sender, eventArgs);
        }

        /// <summary>
        /// 将日志内容显示在屏幕,并记录日志文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void WriteScreenLog(object sender, AlarmHelper.AlermEventArgs eventArgs)
        {
            try
            {
                ListViewItem item = new ListViewItem(
                    new string[] {
                        DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        eventArgs.AlermMsg
                    });
                if (eventArgs.AlermIsErr)
                    item.ForeColor = Color.Red;
                else
                    item.ForeColor = Color.Black;

                //【日志】选项卡
                if (lvTxtLog.Items.Count > configHelper.LogMaxLine)
                {
                    lvTxtLog.Items.RemoveAt(0);
                }
                lvTxtLog.BeginUpdate();
                lvTxtLog.Items.Add(item);
                lvTxtLog.Items[lvTxtLog.Items.Count - 1].EnsureVisible();
                lvTxtLog.EndUpdate();

                //记录日志文件
                LogHelper.WriteLog(eventArgs.AlermName, eventArgs.AlermMsg);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("ElevateX", $"【失败】 [{ex.StackTrace}] 错误 -> [{ex.Message}]");
            }
        }


        private void InitializeTimer()
        {
            //获取任务的运行时间
            int jobRunTime = Convert.ToInt16(numRunTime.Value);
            // 创建一个 30 分钟间隔的定时器 (30 * 60 * 1000 毫秒)
            timer = new System.Timers.Timer(jobRunTime * 60 * 1000);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true; // 设置为true以便重复执行
            timer.Enabled = true;
        }

        private async void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //获取代理列表
            List<ProxyInfo> proxyList = GetProxyList();
            if (proxyList.Count > 0)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    foreach (ListViewItem item in listViewEx1.SelectedItems)
                    {
                        //遍历代理列表
                        foreach (ProxyInfo proxy in proxyList)
                        {
                            string url = item.SubItems[1].Text; // 搜索结果的URL
                            string title = item.SubItems[0].Text; // 搜索结果的标题
                            VisitWebSite(url, title, proxy);
                        }
                    }
                });
            }
        }
    }
    internal struct ProxyInfo
    {
        /// <summary>
        /// 主机IP
        /// </summary>
        public string Host;
        /// <summary>
        /// 主机端口
        /// </summary>
        public int Port;
        /// <summary>
        /// 失效时间
        /// </summary>
        public DateTime ExpireTime;
    }
}
