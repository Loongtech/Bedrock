 
using Net.LoongTech.OmniCoreX;
using System.Reflection;

namespace Net.LoongTech.ElevateX
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //从程序所在目录下，获取所有 和任务有关的 dll 文件
                string jobFilePath = new ConfigHelper().JobFilePath;
                // 搜索模式，寻找所有以"Job_"开头，且扩展名为".dll"的文件
                string searchPattern = "Job_*.dll";
                // 使用GetFiles方法搜索文件，包括所有子目录
                string[] files = Directory.GetFiles(jobFilePath, searchPattern, SearchOption.AllDirectories);

                //从程序所在目录下，获取所有 和任务有关的 dll 文件
                foreach (string jobFile in files)
                {
                    new LogHelper().SendEvent("F10Data_Capture", $"找到任务文件 ->  {jobFile} ", false);
                    Type jobClass = getJobClass(jobFile);
                    if (null != jobClass)
                    {
                        //任务的中文名称
                        string jobName = ReflectionHelper.GetProperty(jobClass, "GetJobName");
                        //任务的运行时间
                        string jobRunTime = ReflectionHelper.GetProperty(jobClass, "GetJobRunTime");

                        /*
                         * 以下代码用于测试，不用在生产环境使用
                         */
                        //根据类型创建实例
                        object obj = Activator.CreateInstance(jobClass, true);
                        //调用方法
                        MethodInfo m1 = jobClass.GetMethod("RunNow");
                        m1.Invoke(obj, null);

                    }
                }

            }
            catch (Exception ex)
            {
                new LogHelper().SendEvent("F10Data_Capture", $"AddQuartzServices -> {ex.Message}", true);
            }
        }


        /// <summary>
        /// 获取DLL 文件中关于 Job 任务类
        /// </summary>
        /// <param name="_dllFilePath">DLL 文件绝对路径</param>
        /// <returns></returns>
        private static Type getJobClass(string _dllFilePath)
        {

            Type returnValue = null;
            try
            {
                if (String.IsNullOrEmpty(_dllFilePath))
                    return returnValue;
                //获取DLL文件中所有的 类 
                Type[] allClass = ReflectionHelper.getClassByFile(_dllFilePath);

                foreach (Type oneClass in allClass)
                {
                    if (oneClass.Name.IndexOf("Job") < 0)
                        continue;
                    else
                        returnValue = oneClass;
                }
            }
            catch (Exception ex)
            {
                new LogHelper().SendEvent("Main", $"getJobClass -> {ex.Message}", true);
            }
            return returnValue;
        }
    }
}