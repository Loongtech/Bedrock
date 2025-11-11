using System.Data;
using System.Reflection;

namespace Net.LoongTech.Bedrock.Core.Extensions
{
    /// <summary>
    /// 提供集合相关的扩展方法。
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// 将一个泛型列表 (List<T>) 高效地转换为 DataTable。
        /// </summary>
        /// <typeparam name="T">列表中的对象类型。</typeparam>
        /// <param name="list">要转换的对象列表。</param>
        /// <returns>一个包含列表数据的 DataTable。</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            // 创建DataTable，表名默认为类型名
            var dataTable = new DataTable(typeof(T).Name);

            // 使用反射获取类型T的所有公共实例属性
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 为DataTable创建列
            foreach (var prop in props)
            {
                // 获取属性的核心类型（处理可空类型，例如 int? 会转为 int）
                Type coreType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dataTable.Columns.Add(prop.Name, coreType);
            }

            // 遍历列表中的每个对象
            foreach (var item in list)
            {
                // 创建一个新行
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    // 获取属性值，并处理DBNull
                    values[i] = props[i].GetValue(item, null) ?? DBNull.Value;
                }
                // 将新行添加到DataTable中
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}