using Dapper;

namespace Net.LoongTech.Bedrock.Data
{
    /// <summary>
    /// 定义了执行高级存储过程的能力，
    /// 例如处理输出参数或返回多个结果集。
    /// </summary>
    public interface IAdvancedDbHelper
    {
        /// <summary>
        /// (高级) 异步执行带复杂参数的存储过程，并返回结果集。
        /// </summary>
        /// <typeparam name="T">要映射的目标类型。</typeparam>
        /// <param name="procedureName">存储过程名称。</param>
        /// <param name="parameters">Dapper 的 DynamicParameters 对象。</param>
        /// <returns>包含查询结果的对象列表。</returns>
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(
            string procedureName,
            DynamicParameters parameters,
            int commandTimeout = 60
        );

        /// <summary>
        /// (高级) 异步执行带复杂参数的存储过程，不返回结果集。
        /// 主要用于有输出参数的场景。
        /// </summary>
        /// <param name="procedureName">存储过程名称。</param>
        /// <param name="parameters">Dapper 的 DynamicParameters 对象。</param>
        Task ExecuteStoredProcedureAsync(
            string procedureName,
            DynamicParameters parameters,
            int commandTimeout = 60
        );
    }
}
