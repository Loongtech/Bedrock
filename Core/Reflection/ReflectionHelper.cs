using System.Reflection;

namespace Net.LoongTech.Bedrock.Core.Reflection
{
    /// <summary>
    /// 提供一系列基于 System.Reflection 的静态辅助方法，用于在运行时动态操作类型和对象。
    /// 反射操作通常性能较低，如果需要频繁调用，建议调用者缓存反射结果（如 MethodInfo, PropertyInfo）。
    /// </summary>
    public static class ReflectionHelper
    {
        #region --- 类型与实例创建 ---

        /// <summary>
        /// 根据程序集名称和完整的类型名称动态加载 Type 对象。
        /// </summary>
        /// <param name="assemblyName">程序集名称（不含 .dll 后缀）。</param>
        /// <param name="fullTypeName">完整的类型名称，包含命名空间（例如 "System.String"）。</param>
        /// <param name="ignoreCase">是否忽略类型名称的大小写。</param>
        /// <returns>加载的 Type 对象，如果找不到则返回 null。</returns>
        public static Type? GetType(string assemblyName, string fullTypeName, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(fullTypeName))
            {
                return null;
            }

            try
            {
                var assembly = Assembly.Load(assemblyName);
                return assembly.GetType(fullTypeName, throwOnError: false, ignoreCase: ignoreCase);
            }
            catch (Exception ex) // 例如 FileNotFoundException
            {
                // 建议记录日志
                Console.WriteLine($"加载程序集 '{assemblyName}' 失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 根据程序集文件路径和完整的类型名称动态加载 Type 对象。
        /// </summary>
        /// <param name="assemblyFilePath">程序集文件的绝对路径（例如 "C:\libs\MyLib.dll"）。</param>
        /// <param name="fullTypeName">完整的类型名称，包含命名空间。</param>
        /// <param name="ignoreCase">是否忽略类型名称的大小写。</param>
        /// <returns>加载的 Type 对象，如果找不到则返回 null。</returns>
        public static Type? GetTypeFromAssemblyFile(string assemblyFilePath, string fullTypeName, bool ignoreCase = false)
        {
            if (string.IsNullOrEmpty(assemblyFilePath) || !File.Exists(assemblyFilePath) || string.IsNullOrEmpty(fullTypeName))
            {
                return null;
            }

            try
            {
                var assembly = Assembly.LoadFile(assemblyFilePath);
                return assembly.GetType(fullTypeName, throwOnError: false, ignoreCase: ignoreCase);
            }
            catch (Exception ex)
            {
                // 建议记录日志
                Console.WriteLine($"从文件 '{assemblyFilePath}' 加载程序集失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 动态创建指定类型的实例。
        /// </summary>
        /// <typeparam name="T">期望返回的对象基类型或接口。</typeparam>
        /// <param name="type">要创建实例的 Type 对象。</param>
        /// <param name="constructorArgs">传递给构造函数的参数。</param>
        /// <returns>创建的实例，如果失败则返回 T 的默认值 (通常是 null)。</returns>
        public static T? CreateInstance<T>(Type type, params object[] constructorArgs)
        {
            if (type == null)
            {
                return default(T);
            }

            try
            {
                object? instance = Activator.CreateInstance(type, constructorArgs);
                return (T?)instance;
            }
            catch (Exception ex)
            {
                // 建议记录日志                 
                Console.WriteLine($"创建类型 '{type.FullName}' 的实例失败: {ex.Message}");
                return default;
            }
        }

        #endregion

        #region --- 属性操作 ---

        /// <summary>
        /// 获取指定对象实例的属性值。
        /// </summary>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">要获取值的属性名称。</param>
        /// <returns>属性的值。如果属性不存在或发生错误，则返回 null。</returns>
        public static object? GetPropertyValue(object instance, string propertyName)
        {
            if (instance == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            try
            {
                var propertyInfo = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                return propertyInfo?.GetValue(instance, null);
            }
            catch (Exception ex)
            {
                // 建议记录日志
                Console.WriteLine($"获取属性 '{propertyName}' 的值失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 设置指定对象实例的属性值。
        /// </summary>
        /// <param name="instance">对象实例。</param>
        /// <param name="propertyName">要设置值的属性名称。</param>
        /// <param name="value">要设置的新值。</param>
        /// <returns>如果设置成功，则为 true；否则为 false。</returns>
        public static bool SetPropertyValue(object instance, string propertyName, object value)
        {
            if (instance == null || string.IsNullOrEmpty(propertyName))
            {
                return false;
            }

            try
            {
                var propertyInfo = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfo == null || !propertyInfo.CanWrite)
                {
                    return false;
                }

                // 进行类型转换，以支持将 string 设置给 int 等类型的属性
                object convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                propertyInfo.SetValue(instance, convertedValue, null);
                return true;
            }
            catch (Exception ex)
            {
                // 建议记录日志
                Console.WriteLine($"设置属性 '{propertyName}' 的值失败: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region --- 方法调用 ---

        /// <summary>
        /// 动态调用指定对象的实例方法。
        /// </summary>
        /// <param name="instance">要调用方法的对象实例。</param>
        /// <param name="methodName">要调用的方法名称。</param>
        /// <param name="parameters">传递给方法的参数数组。</param>
        /// <returns>方法的返回值。如果方法无返回值，则返回 null。如果调用失败，则抛出异常。</returns>
        public static object? InvokeMethod(object instance, string methodName, params object[] parameters)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

            // 获取参数类型数组，用于精确匹配方法签名
            var paramTypes = parameters?.Select(p => p.GetType()).ToArray() ?? Type.EmptyTypes;

            var methodInfo = instance.GetType().GetMethod(methodName, paramTypes);
            if (methodInfo == null)
            {
                // 如果精确匹配失败，可以尝试更宽松的匹配（例如，允许参数类型是基类）
                // 这里为了简化，我们只做精确匹配。
                throw new MissingMethodException(instance.GetType().FullName, methodName);
            }

            return methodInfo.Invoke(instance, parameters);
        }

        /// <summary>
        /// 动态调用指定类型的静态方法。
        /// </summary>
        /// <param name="type">包含静态方法的 Type 对象。</param>
        /// <param name="methodName">要调用的静态方法名称。</param>
        /// <param name="parameters">传递给方法的参数数组。</param>
        /// <returns>方法的返回值。如果方法无返回值，则返回 null。如果调用失败，则抛出异常。</returns>
        public static object? InvokeStaticMethod(Type type, string methodName, params object[] parameters)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));

            var paramTypes = parameters?.Select(p => p.GetType()).ToArray() ?? Type.EmptyTypes;

            var methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, paramTypes, null);
            if (methodInfo == null)
            {
                throw new MissingMethodException(type.FullName, methodName);
            }

            return methodInfo.Invoke(null, parameters); // 对于静态方法，第一个参数是 null
        }

        #endregion

        #region --- 程序集扫描 ---

        /// <summary>
        /// 从当前执行的程序集中获取所有类型。
        /// </summary>
        /// <returns>类型数组。如果出错则返回空数组。</returns>
        public static Type[] GetTypesFromCurrentAssembly()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException ex) // 特殊处理加载类型时的错误
            {
                // 使用 LINQ 的 Where() 方法过滤掉所有 null 元素
                // 这样返回的就是一个纯净的、不含 null 的 Type[] 数组
                return ex.Types.Where(t => t != null).ToArray()!;
            }
        }

        /// <summary>
        /// 从指定的程序集文件中加载并获取所有公共类型。
        /// </summary>
        /// <param name="assemblyFilePath">DLL或EXE文件的绝对路径。</param>
        /// <returns>类型数组。如果文件不存在或加载失败，则返回空数组。</returns>
        public static Type[] GetTypesFromAssemblyFile(string assemblyFilePath)
        {
            if (!File.Exists(assemblyFilePath))
            {
                return Type.EmptyTypes;
            }

            try
            {
                var assembly = Assembly.LoadFile(assemblyFilePath);
                return assembly.GetExportedTypes();
            }
            catch (Exception ex)
            {
                // 建议记录日志
                Console.WriteLine($"从文件 '{assemblyFilePath}' 加载类型失败: {ex.Message}");
                return Type.EmptyTypes;
            }
        }

        /// <summary>
        /// 在指定的程序集中，查找特定命名空间下的所有类型。
        /// </summary>
        /// <param name="assembly">要搜索的程序集。</param>
        /// <param name="namespace">要匹配的命名空间。</param>
        /// <returns>符合条件的类型数组。</returns>
        public static Type[] GetTypesByNamespace(Assembly assembly, string @namespace)
        {
            if (assembly == null || string.IsNullOrEmpty(@namespace))
            {
                return Type.EmptyTypes;
            }

            return assembly.GetTypes()
                           .Where(t => string.Equals(t.Namespace, @namespace, StringComparison.Ordinal))
                           .ToArray();
        }

        /// <summary>
        /// 在指定的程序集文件中，查找所有实现了特定接口或继承自特定基类的类型。
        /// </summary>
        /// <typeparam name="TBase">要查找的基类型（可以是接口或类）。</typeparam>
        /// <param name="assemblyFilePath">DLL或EXE文件的绝对路径。</param>
        /// <returns>所有符合条件的具体实现类型（非抽象类）。</returns>
        public static IEnumerable<Type> FindImplementationsOf<TBase>(string assemblyFilePath)
        {
            var baseType = typeof(TBase);
            var allTypes = GetTypesFromAssemblyFile(assemblyFilePath);

            return allTypes.Where(t =>
                baseType.IsAssignableFrom(t) && // 判断 t 是否继承或实现了 TBase
                !t.IsInterface &&              // 排除接口本身
                !t.IsAbstract);                // 排除抽象基类
        }

        #endregion
    }
}