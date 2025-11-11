using System.Data;

namespace Net.LoongTech.Bedrock.Data
{
    public interface IDbHelper
    {
        /// <summary>
        /// 异步执行查询，并将结果映射到指定类型的对象列表。
        /// </summary>
        /// <typeparam name="T">要映射的目标类型。</typeparam>
        /// <param name="sql">要执行的 SQL 查询。</param>
        /// <param name="parameters">查询参数（可选），使用匿名对象。</param>
        /// <returns>包含查询结果的对象列表。</returns>
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, int commandTimeout = 30);

        /// <summary>
        /// 异步执行查询，并返回第一行结果，如果无结果则返回默认值。
        /// </summary>
        Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null, int commandTimeout = 30);

        /// <summary>
        /// 异步执行非查询 SQL 语句（如 INSERT, UPDATE, DELETE）。
        /// </summary>
        /// <param name="sql">要执行的 SQL 语句。</param>
        /// <param name="parameters">查询参数（可选），使用匿名对象。</param>
        /// <returns>受影响的行数。</returns>
        Task<int> ExecuteAsync(string sql, object? parameters = null, int commandTimeout = 30);

        /// <summary>
        /// 异步执行标量查询并返回单个值。
        /// </summary>
        /// <typeparam name="T">标量值的类型。</typeparam>
        /// <param name="sql">要执行的 SQL 查询。</param>
        /// <param name="parameters">查询参数（可选），使用匿名对象。</param>
        /// <returns>标量值。</returns>
        Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null, int commandTimeout = 30);

        // --- 向后兼容的 DataTable 方法 ---
        /// <summary>
        /// (兼容旧代码) 异步执行查询，并将结果返回为 DataTable。
        /// </summary>
        /// <param name="sql">要执行的 SQL 查询或存储过程名称。</param>
        /// <param name="parameters">查询参数（可选），使用匿名对象。</param>
        /// <param name="commandType">命令类型（文本或存储过程）。</param>
        /// <returns>包含查询结果的 DataTable。</returns>
        Task<DataTable> ExecuteDataTableAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text, int commandTimeout = 30);

        /// <summary>
        /// (推荐) 异步执行多条 SQL 语句（事务）。
        /// 这种方式比传递字符串列表更安全，因为它支持参数化查询，能防止 SQL 注入。
        /// </summary>
        /// <param name="commands">包含 SQL 语句和参数的命令元组列表。</param>
        /// <param name="commandTimeout">超时时间 默认30秒</param>
        /// <returns>总共受影响的行数</returns>
        Task<int> ExecuteTransactionAsync(IEnumerable<(string sql, object parameters)> commands, int commandTimeout = 30);

        /// <summary>
        /// 为特定数据库的 LIKE 查询处理特殊字符。
        /// 调用者应根据具体数据库的需要，可能要在 SQL 中配合使用 ESCAPE 子句。
        /// </summary>
        /// <param name="source">原始搜索字符串。</param>
        /// <returns>转义后的、可用于 LIKE 子句的字符串。</returns>
        string HandleLikeKey(string? source);
    }
}
