using System.Reflection;

namespace Net.LoongTech.OmniCoreX
{
    /// <summary>
    /// 利用反射动态执行类中的方法
    /// </summary>
    public class ReflectionHelper
    {
        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>        
        /// <param name="nameSpace">命名空间</param>
        /// <param name="className">类名</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string nameSpace, string className, string assemblyName)
        {
            try
            {
                //命名空间.类名,程序集
                string path = nameSpace + "." + className + "," + assemblyName;
                //加载类型
                Type type = Type.GetType(path);
                //根据类型创建实例
                object obj = Activator.CreateInstance(type, true);
                //类型转换并返回
                return (T)obj;
            }
            catch
            {
                //发生异常时，返回类型的默认值。
                return default(T);
            }
        }

        /// <summary>
        /// 调用方法实例
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam> 
        /// <param name="nameSpace">命名空间</param>
        /// <param name="className">类名</param>
        /// <param name="methodName">方法名</param>
        /// <param name="assemblyName">程序集名称</param>
        /// <param name="paras">方法的参数</param>
        /// <returns></returns>
        public static T GetInvokeMethod<T>(string nameSpace, string className, string methodName, string assemblyName, object[] paras)
        {
            try
            {
                //命名空间.类名,程序集
                string path = nameSpace + "." + className + "," + assemblyName;
                //加载类型
                Type type = Type.GetType(path);
                //根据类型创建实例
                object obj = Activator.CreateInstance(type, true);
                //加载方法参数类型及方法
                MethodInfo method = null;
                if (paras != null && paras.Length > 0)
                {
                    //加载方法参数类型
                    Type[] paratypes = new Type[paras.Length];
                    for (int i = 0; i < paras.Length; i++)
                    {
                        paratypes[i] = paras[i].GetType();
                    }
                    //加载有参方法
                    method = type.GetMethod(methodName, paratypes);
                }
                else
                {
                    //加载无参方法
                    method = type.GetMethod(methodName);
                }
                //类型转换并返回
                return (T)method.Invoke(obj, paras);
            }
            catch
            {
                //发生异常时，返回类型的默认值。
                return default(T);
            }
        }

        /// <summary>
        /// 调用方法实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">对象</param>
        /// <param name="methodName">方法名</param>
        /// <param name="paras">方法的参数</param>
        /// <returns></returns>
        public static T GetInvokeMethod<T>(Type type, string methodName, object[] paras)
        {
            try
            {

                //根据类型创建实例
                object obj = Activator.CreateInstance(type, true);
                //加载方法参数类型及方法
                MethodInfo method = null;
                if (paras != null && paras.Length > 0)
                {
                    //加载方法参数类型
                    Type[] paratypes = new Type[paras.Length];
                    for (int i = 0; i < paras.Length; i++)
                    {
                        paratypes[i] = paras[i].GetType();
                    }
                    //加载有参方法
                    method = type.GetMethod(methodName, paratypes);
                }
                else
                {
                    //加载无参方法
                    method = type.GetMethod(methodName);
                }
                //类型转换并返回
                return (T)method.Invoke(obj, paras);
            }
            catch
            {
                //发生异常时，返回类型的默认值。
                return default(T);
            }
        }

        /// <summary>
        /// 获取类 
        /// </summary>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="className">类名</param> 
        /// <param name="assemblyName">程序集名称</param> 
        /// <returns></returns>
        public static Type GetClass(string nameSpace, string className, string assemblyName)
        {
            try
            {
                //命名空间.类名,程序集
                string path = nameSpace + "." + className + "," + assemblyName;
                //加载类型
                Type type = Type.GetType(path);
                //根据类型创建实例
                //object obj = Activator.CreateInstance(type, true);
                return type;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// 获取类中的属性的值
        /// </summary>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="className">类名</param>
        /// <param name="assemblyName">程序集名称</param> 
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static string GetProperty(string nameSpace, string className, string assemblyName, string propertyName)
        {
            try
            {
                //命名空间.类名,程序集
                string path = nameSpace + "." + className + "," + assemblyName;
                //string path = nameSpace + "." + className;
                //加载类型
                Type type = Type.GetType(path);
                //根据类型创建实例
                object obj = Activator.CreateInstance(type, true);
                object o = type.GetProperty(propertyName).GetValue(obj, null);
                string Value = Convert.ToString(o);
                if (string.IsNullOrEmpty(Value))
                    return string.Empty;
                return Value;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取类 中属性的值
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetProperty(Type typeName, string propertyName)
        {
            try
            {
                //根据类型创建实例
                object obj = Activator.CreateInstance(typeName, true);
                if (null == typeName.GetProperty(propertyName))
                    return string.Empty;
                object o = typeName.GetProperty(propertyName).GetValue(obj, null);
                string Value = Convert.ToString(o);
                if (string.IsNullOrEmpty(Value))
                    return string.Empty;
                return Value;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 获取类 中属性(数组类型)的值
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string[] GetPropertys(Type typeName, string propertyName)
        {
            try
            {
                //根据类型创建实例
                object obj = Activator.CreateInstance(typeName, true);
                if (null == typeName.GetProperty(propertyName))
                    return null;
                object o = typeName.GetProperty(propertyName).GetValue(obj, null);
                if (o == null) return null;

                string[] array = null;
                if (o.GetType().IsArray)
                {
                    array = (string[])o;
                }
                else
                {
                    array = new string[1];
                    array[0] = Convert.ToString(o);
                }
                return array;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// 设置类中的属性的值
        /// </summary>
        /// <param name="nameSpace">命名空间</param>
        /// <param name="className">类名</param>
        /// <param name="assemblyName">程序集名称</param> 
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns></returns>
        public static bool SetModelValue(string nameSpace, string className, string assemblyName, string propertyName, string propertyValue)
        {
            try
            {
                //命名空间.类名,程序集
                string path = nameSpace + "." + className + "," + assemblyName;
                //加载类型
                Type type = Type.GetType(path);
                //根据类型创建实例
                object obj = Activator.CreateInstance(type, true);
                object v = Convert.ChangeType(propertyValue, type.GetProperty(propertyName).PropertyType);
                type.GetProperty(propertyName).SetValue(obj, v, null);
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 设置类中的属性的值
        /// </summary>
        /// <param name="typeName">命名空间</param> 
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns></returns>
        public static bool SetModelValue(Type typeName, string propertyName, string propertyValue)
        {
            try
            {

                //根据类型创建实例
                object obj = Activator.CreateInstance(typeName, true);
                object v = Convert.ChangeType(propertyValue, typeName.GetProperty(propertyName).PropertyType);
                typeName.GetProperty(propertyName).SetValue(obj, v, null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取 Task 命名空间下所有的类
        /// </summary>
        /// <returns></returns>
        public static Type[] getAllTaskClass()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetTypes();
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// 获取 指定文件中的所有类
        /// </summary>
        /// <param name="_filePathName">文件路径及文件名</param>
        /// <returns></returns>
        public static Type[] getClassByFile(string _filePathName)
        {
            if (File.Exists(_filePathName))
            {
                Assembly assembly = Assembly.LoadFile(_filePathName);
                return assembly.GetExportedTypes();
            }
            else
            {
                throw new Exception("指定的文件不存在");
            }
        }
        /// <summary>
        /// 根据 程序集和命名空间来获取所有类
        /// </summary>
        /// <param name="assembly">程序集</param>
        /// <param name="nameSpace">命名空间</param>
        /// <returns></returns>
        public static Type[] getTypesByNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        /// <summary>
        /// 获取DLL 文件中关于 Job 任务类
        /// </summary>
        /// <param name="_dllFilePath">DLL 文件绝对路径</param>
        /// <returns></returns>
        public static Type getJobClass(string _dllFilePath)
        {
            Type returnValue = null;
            if (String.IsNullOrEmpty(_dllFilePath))
                return returnValue;
            //获取DLL文件中所有的 类 
            Type[] allClass = getClassByFile(_dllFilePath);

            foreach (Type oneClass in allClass)
            {
                if (oneClass.Name.IndexOf("Job") < 0)
                    continue;
                else
                    returnValue = oneClass;
            }
            return returnValue;
        }


    }
}
