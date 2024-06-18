using LitJson;
using Microsoft.Web.WebView2.Core;
using Net.LoongTech.OmniCoreX;

namespace Net.LoongTech.ElevateX
{
    public partial class FrmMain : Form
    {
        //配置文件获取类
        private ConfigHelper configHelper = new();


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

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBox1.Text))
            {

                string url =
                    @"https://www.baidu.com/s?" +
                    @"ie=utf-8&f=8&rsv_bp=1&tn=baidu&rsv_pq=dac3e43a000957e6&rsv_t=8db1tuoOF1r1dy50%2BjHlWJxmjiif5PSgr8XvoKt3S8UZxK9wC0Bn8pmahPk&rqlang=cn&rsv_dl=tb&rsv_enter=1&rsv_sug3=19&rsv_sug1=1&rsv_sug7=100&rsv_sug2=0&rsv_btype=t&inputT=6393&rsv_sug4=6707" +
                    $"&wd={textBox1.Text}&oq={textBox1.Text}";
                webView21.CoreWebView2.Navigate(url);

                webView21.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
            }
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {

            if (webView21.CoreWebView2.Source.Contains("www.baidu.com/s"))
            {
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
                string result = await webView21.CoreWebView2.ExecuteScriptAsync(script);
                result = result.Replace("\\\"", "'").Replace("\"", "");

                try
                {
                    JsonData jsonData = JsonMapper.ToObject(result);
                    listViewEx1.Items.Clear();
                    listViewEx1.BeginUpdate();
                    foreach (JsonData item in jsonData)
                    {
                        listViewEx1.Items.Add(new ListViewItem(new[] { item["title"].ToString(), item["url"].ToString() }));
                    }
                    listViewEx1.EndUpdate();

                }
                catch (JsonException jsonEx)
                {
                    SendEvent("ElevateX", $"JSON解析错误: {jsonEx.Message}",true);                    
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (listViewEx1.SelectedItems.Count > 0)
            {
                //获取代理列表
                List<ProxyInfo> proxyList = GetProxyList();
                foreach (ListViewItem item in listViewEx1.SelectedItems)
                {
                    //遍历代理列表
                    foreach (ProxyInfo proxy in proxyList)
                    {
                        string proxyString = $"{proxy.Host}:{proxy.Port}"; // 代理字符串
                        string url = item.SubItems[1].Text; // 搜索结果的URL
                        FrmView f = new FrmView(url, proxyString);
                        f.Show();
                    }
                }
            }
            else
                MessageBox.Show("请从右边的列表中至少选择一个项目");
        }


        private List<ProxyInfo> GetProxyList()
        {
            List<ProxyInfo> returnValue = new List<ProxyInfo>();
            try
            {
                string Url_Proxy = new ConfigHelper().GetProxyList;
                if (string.IsNullOrEmpty(Url_Proxy))
                {
                    MessageBox.Show("未配置代理列表");
                    return null;
                }
                else
                {

                    using var httpClient = new HttpClient();

                    var response = httpClient.GetAsync(Url_Proxy).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content.ReadAsStringAsync().Result;

                        JsonData jsonData = JsonMapper.ToObject(content);

                        foreach (JsonData item in jsonData)
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
                    else
                    {
                        throw new Exception(response.StatusCode.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                SendEvent("ElevateX",$"Error:{ex.Message}",true);
            }
            return returnValue;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            //订阅事件
            AlarmHelper alarm = AlarmHelper.Instance;
            alarm.AlarmEvent += new AlarmHelper.AlarmEventHandler(JobAlarmEvent);
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
            this.BeginInvoke(new AlarmHelper.AlarmEventHandler(writeScreenLog), sender, eventArgs);
        }

        /// <summary>
        /// 将日志内容显示在屏幕,并记录日志文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void writeScreenLog(object sender, AlarmHelper.AlermEventArgs eventArgs)
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
                LogHelper.WriteLog("ElevateX", $"【Error】 [{ex.StackTrace}] 错误 -> [{ex.Message}]");
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
}
