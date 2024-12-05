namespace Net.LoongTech.OmniCoreX
{
    public class LogHelper
    {

        /// <summary>
        /// 消息发送事件
        /// </summary>
        /// <param name="jobName">任务名称</param>
        /// <param name="jobMsg">任务消息内容</param>
        /// <param name="isErr">消息类型 true:错误，false：一般提示消息</param>
        public void SendEvent(string jobName, string jobMsg, bool isErr)
        {
            AlarmHelper.AlermEventArgs e = new AlarmHelper.AlermEventArgs(jobName, jobMsg, isErr);
            AlarmHelper.Instance.SendEvent(e);
        }

        #region 在LOG目录下记录日志文件

        /// <summary>
        /// 在LOG目录下的不同的子目录录中生成日志文件
        /// </summary>
        /// <param name="dirName">目录名称(自动创建)</param>
        /// <param name="logContent">日志内容</param>
        public static void WriteLog(string dirName, string logContent)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Log/" + dirName + "/"))
            {
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + dirName + "/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + logContent);
                sw.Dispose();
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Log/" + dirName + "/");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + dirName + "/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + logContent);
                sw.Dispose();
            }
        }

        /// <summary>
        /// 在LOG目录下中生成日志文件
        /// </summary>
        /// <param name="logContent">日志内容</param>

        public static void WriteLog(string logContent)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "Log/"))
            {
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + logContent);
                sw.Dispose();
            }
            else
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "Log/");
                StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Log/" + DateTime.Now.ToString("yyyyMMdd") + ".log", true, System.Text.Encoding.Unicode);
                sw.WriteLine(DateTime.Now.ToString() + "\t" + logContent);
                sw.Dispose();
            }
        }

        private void saveFile(string filePath, string fileName, string content)
        {
            if (Directory.Exists(filePath))
            {
                StreamWriter sw = new(filePath + @"\" + fileName, true, System.Text.Encoding.Unicode);
                sw.WriteLine(content);
                sw.Dispose();
            }
            else
            {
                Directory.CreateDirectory(filePath);
                StreamWriter sw = new(filePath + @"\" + fileName, true, System.Text.Encoding.Unicode);
                sw.WriteLine(content);
                sw.Dispose();
            }
        }

        #endregion 在LOG目录下记录日志文件
    }
}
