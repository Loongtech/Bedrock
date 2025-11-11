using System;

namespace Net.LoongTech.Bedrock.Core.Types
{
    /// <summary>
    /// 提供与 System.Type 相关的静态辅助方法。
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 判断一个类型是否为可空类型 (例如 int?, string, 或其他引用类型)。
        /// </summary>
        /// <param name="type">要检查的类型。</param>
        /// <returns>如果类型是可空的，则为 true；否则为 false。</returns>
        public static bool IsNullable(Type type)
        {
            // 引用类型总是可空的
            if (!type.IsValueType)
            {
                return true;
            }
            // 值类型中，只有 Nullable<T> 是可空的
            return Nullable.GetUnderlyingType(type) != null;
        }

        /// <summary>
        /// 获取一个类型的核心基础类型。如果类型是 Nullable<T>，则返回 T。
        /// </summary>
        /// <param name="type">要检查的类型。</param>
        /// <returns>类型的核心基础类型。</returns>
        public static Type GetCoreType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // 如果是可空值类型，返回其基础类型，否则直接返回原类型
            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}