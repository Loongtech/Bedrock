using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace Net.LoongTech.Bedrock.Data
{
    /// <summary>
    /// 使用 Dapper 的现代化 Oracle 数据库操作辅助类。
    /// 实现了通用的 IDbHelper 接口。
    /// </summary>
    public class OracleHelper : IDbHelper, IAdvancedDbHelper // 实现基础数据库操作接口和高级数据库操作接口
    {
        // 存储数据库连接字符串
        private readonly string _connectionString;
        // 日志记录器实例
        private readonly ILogger _logger;

        /// <summary>
        /// 初始化 OracleHelper 的新实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="logger">日志记录器</param>
        public OracleHelper(string connectionString, ILogger logger)
        {
            // 检查并保存连接字符串，如果为 null 则抛出异常
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            // 检查并保存日志记录器，如果为 null 则抛出异常
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 内部方法，用于创建新的 Oracle 数据库连接
        private OracleConnection CreateConnection()
        {
            // 返回一个新的 OracleConnection 实例
            return new OracleConnection(_connectionString);
        }

        #region IDbHelper 接口实现

        /// <summary>
        /// 异步执行查询并返回结果集
        /// </summary>
        /// <typeparam name="T">结果集元素类型</typeparam>
        /// <param name="sql">SQL 查询语句</param>
        /// <param name="parameters">查询参数（可选）</param>
        /// <returns>查询结果集</returns>
        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                // 创建数据库连接，使用 using 确保连接正确释放
                await using var connection = CreateConnection();
                // 使用 Dapper 执行查询，Dapper 自动处理 Oracle 的 :param 风格参数
                return await connection.QueryAsync<T>(
                    sql,
                    parameters,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                // 记录错误日志
                _logger.LogError(ex, "执行 Oracle 查询失败。SQL: {Sql}", sql);
                // 重新抛出异常
                throw;
            }
        }

        /// <summary>
        /// 异步执行查询并返回第一行结果
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="sql">SQL 查询语句</param>
        /// <param name="parameters">查询参数（可选）</param>
        /// <returns>查询结果的第一行，如果没有结果则返回默认值</returns>
        public async Task<T?> QueryFirstOrDefaultAsync<T>(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                // 创建数据库连接
                await using var connection = CreateConnection();
                // 使用 Dapper 查询第一行结果
                return await connection.QueryFirstOrDefaultAsync<T>(
                    sql,
                    parameters,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                // 记录错误日志
                _logger.LogError(ex, "执行 Oracle 单行查询失败。SQL: {Sql}", sql);
                // 重新抛出异常
                throw;
            }
        }

        /// <summary>
        /// 异步执行非查询 SQL 语句（如 INSERT、UPDATE、DELETE）
        /// </summary>
        /// <param name="sql">SQL 语句</param>
        /// <param name="parameters">SQL 参数（可选）</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteAsync(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                // 创建数据库连接
                await using var connection = CreateConnection();
                // 使用 Dapper 执行 SQL 语句
                return await connection.ExecuteAsync(
                    sql,
                    parameters,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                // 记录错误日志
                _logger.LogError(ex, "执行 Oracle 非查询操作失败。SQL: {Sql}", sql);
                // 重新抛出异常
                throw;
            }
        }

        /// <summary>
        /// 异步执行标量查询（返回单个值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">SQL 查询语句</param>
        /// <param name="parameters">查询参数（可选）</param>
        /// <returns>查询结果的第一个值</returns>
        public async Task<T?> ExecuteScalarAsync<T>(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                // 创建数据库连接
                await using var connection = CreateConnection();
                // 使用 Dapper 执行标量查询
                return await connection.ExecuteScalarAsync<T>(
                    sql,
                    parameters,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                // 记录错误日志
                _logger.LogError(ex, "执行 Oracle 标量查询失败。SQL: {Sql}", sql);
                // 重新抛出异常
                throw;
            }
        }

        /// <summary>
        /// 异步执行查询并将结果填充到 DataTable 中
        /// </summary>
        /// <param name="sql">SQL 查询语句</param>
        /// <param name="parameters">查询参数（可选）</param>
        /// <param name="commandType">命令类型，默认为 Text</param>
        /// <returns>包含查询结果的 DataTable</returns>
        public async Task<DataTable> ExecuteDataTableAsync(
            string sql,
            object? parameters = null, CommandType commandType = CommandType.Text,
            int commandTimeout = 30
        )
        {
            try
            {
                // 创建数据库连接
                await using var connection = CreateConnection();
                // 执行查询并获取数据读取器
                await using var reader = await connection.ExecuteReaderAsync(
                    sql,
                    parameters,
                    commandType: commandType,
                    commandTimeout: commandTimeout
                );

                // 创建 DataTable 并加载数据
                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception ex)
            {
                // 记录错误日志
                _logger.LogError(ex, "执行 Oracle 查询并填充 DataTable 失败。SQL: {Sql}", sql);
                // 重新抛出异常
                throw;
            }
        }

        #endregion

        #region IAdvancedDbHelper 实现 存储过程处理 (使用核心 Dapper)

        /// <summary>
        /// (高级) 异步执行带输出参数或返回游标 (REF CURSOR) 的存储过程。
        /// </summary>
        /// <typeparam name="T">返回结果集的类型。</typeparam>
        /// <param name="procedureName">存储过程名称。</param>
        /// <param name="parameters">Dapper 的 DynamicParameters 对象。</param>
        /// <returns>包含结果集（如果有）的对象列表。</returns>
        /// <example>
        /// // 返回 REF CURSOR
        /// var parameters = new DynamicParameters(); // <--- 使用核心 Dapper 的 DynamicParameters
        /// parameters.Add("p_cursor", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
        /// var results = await helper.ExecuteStoredProcedureAdvancedAsync<MyClass>("GetEmployees", parameters);
        /// 
        /// // 输出参数
        /// parameters.Add("p_output", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, size: 100);
        /// await helper.ExecuteStoredProcedureAdvancedAsync<dynamic>("GetEmployeeName", parameters);
        /// var name = parameters.Get<string>("p_output");
        /// </example>
        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(
            string procedureName,
            DynamicParameters parameters,
            int commandTimeout = 60
        )
        {
            try
            {
                // 创建数据库连接
                await using var connection = CreateConnection();
                // 直接使用 Dapper 核心库的 QueryAsync 执行存储过程
                return await connection.QueryAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                // 记录错误日志
                _logger.LogError(ex, "执行 Oracle 高级存储过程 {ProcedureName} 失败。", procedureName);
                // 重新抛出异常
                throw;
            }
        }


        /// <summary>
        /// 执行 Oracle 高级存储过程（无返回值）
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">参数集合</param>
        /// <returns>异步任务</returns>
        public async Task ExecuteStoredProcedureAsync(
            string procedureName,
            DynamicParameters parameters,
            int commandTimeout = 60
        )
        {
            try
            {
                // 创建数据库连接
                await using var connection = CreateConnection();
                // 使用 Dapper 执行存储过程
                await connection.ExecuteAsync(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                // 记录错误日志
                _logger.LogError(ex, "执行 Oracle 高级存储过程 {ProcedureName} 失败。", procedureName);
                // 重新抛出异常
                throw;
            }
        }

        #endregion


        #region 事务处理

        /// <summary>
        /// (推荐) 异步执行多条 SQL 语句（事务）。
        /// </summary>
        /// <param name="commands">SQL 命令集合，每个元素包含 SQL 语句和参数</param>
        /// <returns>受影响的总行数</returns>
        public async Task<int> ExecuteTransactionAsync(
            IEnumerable<(string sql, object parameters)> commands,
            int commandTimeout = 30
        )
        {
            // 记录受影响的总行数
            var totalRowsAffected = 0;
            // 创建数据库连接
            await using var connection = CreateConnection();
            // 打开数据库连接
            await connection.OpenAsync();
            // 开始数据库事务
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 遍历所有命令并执行
                foreach (var (sql, parameters) in commands)
                {
                    // 执行命令并累加受影响行数
                    totalRowsAffected += await connection.ExecuteAsync(
                        sql,
                        parameters,
                        transaction,
                        commandTimeout: commandTimeout
                    );
                }
                // 提交事务
                await transaction.CommitAsync();
                // 记录成功日志
                _logger.LogInformation("Oracle 事务执行成功，共影响 {Rows} 行。", totalRowsAffected);
                // 返回受影响的总行数
                return totalRowsAffected;
            }
            catch (Exception ex)
            {
                // 发生异常时回滚事务
                await transaction.RollbackAsync();
                // 记录错误日志
                _logger.LogError(ex, "Oracle 事务执行失败，已回滚。");
                // 重新抛出异常
                throw;
            }
        }

        #endregion

        /// <summary>
        /// 处理 LIKE 查询中的特殊字符转义
        /// </summary>
        /// <param name="source">原始搜索字符串</param>
        /// <returns>转义后的 LIKE 模式字符串</returns>
        public string HandleLikeKey(string? source)
        {
            // 如果输入为空或空白字符，返回通配符
            if (string.IsNullOrWhiteSpace(source)) return "%";

            // 转义 Oracle 中的特殊字符
            // 注意：在 SQL 中需要配合使用 ESCAPE '\' 子句
            var escaped = source
                .Replace("\\", "\\\\")  // 转义反斜杠
                .Replace("%", "\\%")    // 转义百分号
                .Replace("_", "\\_");   // 转义下划线

            // 添加通配符返回模糊匹配模式
            return $"%{escaped}%";
        }
    }
}
