using Microsoft.Web.WebView2.Core;

namespace Net.LoongTech.ElevateX
{
    public partial class FrmView : Form
    {
        string url = string.Empty;
        string Proxy = string.Empty;
        public FrmView(string _url, string _Proxy)
        {
            url = _url;
            Proxy = _Proxy;
            InitializeComponent();
            
        }

        private async void InitializeAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(Proxy))
                {
                    var options = new CoreWebView2EnvironmentOptions($"--proxy-server=http={Proxy};https={Proxy}");
                    var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                    await webView21.EnsureCoreWebView2Async(environment);
                }
                else
                {                  
                    await webView21.EnsureCoreWebView2Async(null);
                }
                webView21.CoreWebView2.Navigate(url);

                webView21.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;

            }
            catch { }
        }

        private async void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // 确保这是你想要的操作，立即关闭可能不是最佳用户体验
                this.Close();
            }
        }

        private void FrmView_Load(object sender, EventArgs e)
        {
            InitializeAsync();
        }
    }
}
