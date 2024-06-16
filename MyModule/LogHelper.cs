namespace Net.LoongTech.OmniCoreX
{
    public class LogHelper
    {

        /// <summary>
        /// 消息发送事件
        /// </summary>
        /// <param name="_jobName">任务名称</param>
        /// <param name="_JobMsg">任务消息内容</param>
        /// <param name="_IsErr">消息类型 true:错误，false：一般提示消息</param>
        public void SendEvent(string _jobName, string _JobMsg, bool _IsErr)
        {
            AlarmHelper.AlermEventArgs e = new AlarmHelper.AlermEventArgs(_jobName, _JobMsg, _IsErr);
            AlarmHelper.Instance.SendEvent(e);
        }

        #region 在LOG目录下记录日志文件

        /// <summary>
        /// 在LOG目录下的不同的子目录录中生成日志文件
        /// </summary>
        /// <param name="_dirName">目录名称(自动创建)</param>
        /// <param name="_logContent">日志内容</param>
        public static void WriteLog(string _dirName, string _logContent)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Log/" + _dirName + "/"))
            {
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + _dirName + "/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + _logContent);
                sw.Dispose();
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Log/" + _dirName + "/");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + _dirName + "/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + _logContent);
                sw.Dispose();
            }
        }

        /// <summary>
        /// 在LOG目录下中生成日志文件
        /// </summary>
        /// <param name="_logContent">日志内容</param>

        public static void WriteLog(string _logContent)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Log/"))
            {
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + _logContent);
                sw.Dispose();
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Log/");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + _logContent);
                sw.Dispose();
            }
        }

        private void saveFile(string _filePath, string _fileName, string _content)
        {
            if (Directory.Exists(_filePath))
            {
                StreamWriter sw = new(_filePath + @"\" + _fileName, true, System.Text.Encoding.Unicode);
                sw.WriteLine(_content);
                sw.Dispose();
            }
            else
            {
                Directory.CreateDirectory(_filePath);
                StreamWriter sw = new(_filePath + @"\" + _fileName, true, System.Text.Encoding.Unicode);
                sw.WriteLine(_content);
                sw.Dispose();
            }
        }

        #endregion 在LOG目录下记录日志文件
    }
}
