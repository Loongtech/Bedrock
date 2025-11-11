using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Net.LoongTech.Bedrock.Data
{
    /// <summary>
    /// 使用 Dapper 的现代化 SQL Server 数据库操作辅助类。
    /// 实现了通用的 IDbHelper 接口，并提供了对存储过程和事务的强大支持。
    /// </summary>
    public class SqlServerHelper : IDbHelper, IAdvancedDbHelper // <--- 实现接口
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        /// <summary>
        /// 初始化 SqlServerHelper 的新实例。
        /// </summary>
        public SqlServerHelper(string connectionString, ILogger logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 内部方法，用于创建连接
        private SqlConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        #region IDbHelper 接口实现

        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? parameters = null,
             int commandTimeout = 30
        )
        {
            try
            {
                await using var connection = CreateConnection();
                return await connection.QueryAsync<T>(
                    sql,
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 SQL Server 查询失败。SQL: {Sql}", sql);
                throw;
            }
        }

        public async Task<T?> QueryFirstOrDefaultAsync<T>(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                await using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<T>(
                    sql,
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 SQL Server 单行查询失败。SQL: {Sql}", sql);
                throw;
            }
        }

        public async Task<int> ExecuteAsync(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                await using var connection = CreateConnection();
                return await connection.ExecuteAsync(
                    sql,
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 SQL Server 非查询操作失败。SQL: {Sql}", sql);
                throw;
            }
        }

        public async Task<T?> ExecuteScalarAsync<T>(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                await using var connection = CreateConnection();
                return await connection.ExecuteScalarAsync<T>(
                    sql,
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 SQL Server 标量查询失败。SQL: {Sql}", sql);
                throw;
            }
        }

        public async Task<DataTable> ExecuteDataTableAsync(
            string sql,
            object? parameters = null,
            CommandType commandType = CommandType.Text,
            int commandTimeout = 30
        )
        {
            try
            {
                await using var connection = CreateConnection();
                await using var reader = await connection.ExecuteReaderAsync(
                    sql,
                    parameters,
                    commandType: commandType,
                    commandTimeout: commandTimeout
                );

                var dataTable = new DataTable();
                dataTable.Load(reader);

                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 SQL Server 查询并填充 DataTable 失败。SQL: {Sql}", sql);
                throw;
            }
        }

        #endregion

        #region IAdvancedDbHelper 实现 存储过程处理 (使用核心 Dapper)

        /// <summary>
        /// 异步执行 SQL Server 存储过程并返回结果集。
        /// </summary>
        public async Task<IEnumerable<T>> ExecuteStoredProcedureAsync<T>(
            string procedureName,
            DynamicParameters parameters,
            int commandTimeout = 60
        )
        {
            try
            {
                await using var connection = CreateConnection();
                // Dapper 的 QueryAsync 完美支持存储过程
                return await connection.QueryAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询 SQL Server 高级存储过程 {ProcedureName} 失败。", procedureName);
                throw;
            }
        }

        /// <summary>
        /// 异步执行 SQL Server 存储过程，通常用于包含 OUTPUT 参数或返回值的场景。
        /// </summary>
        public async Task ExecuteStoredProcedureAsync(
            string procedureName,
            DynamicParameters parameters,
            int commandTimeout = 60
        )
        {
            try
            {
                await using var connection = CreateConnection();
                // ExecuteAsync 可以执行并填充 OUTPUT 参数
                await connection.ExecuteAsync(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 SQL Server 高级存储过程 {ProcedureName} 失败。", procedureName);
                throw;
            }
        }
        /// <summary>
        /// (高级) 异步执行带输出参数的存储过程。
        /// </summary>
        /// <param name="procedureName">存储过程名称。</param>
        /// <param name="parameters">Dapper 的 DynamicParameters 对象，用于定义输出参数。</param>
        /// <returns>受影响的行数。</returns>
        /// <example>
        /// var parameters = new DynamicParameters();
        /// parameters.Add("@InputParam", 123);
        /// parameters.Add("@OutputParam", dbType: DbType.Int32, direction: ParameterDirection.Output);
        /// await helper.ExecuteStoredProcedureWithOutputAsync("MyProc", parameters);
        /// var outputValue = parameters.Get<int>("@OutputParam");
        /// </example>
        public async Task<int> ExecuteStoredProcedureWithOutputAsync(
            string procedureName,
            DynamicParameters parameters,
            int commandTimeout = 60
        )
        {
            try
            {
                await using var connection = CreateConnection();
                return await connection.ExecuteAsync(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行带输出参数的 SQL Server 存储过程 {ProcedureName} 失败。", procedureName);
                throw;
            }
        }

        #endregion

        #region 事务处理

        /// <summary>
        /// (推荐) 异步执行多条 SQL 语句（事务）。
        /// </summary>
        public async Task<int> ExecuteTransactionAsync(
            IEnumerable<(string sql, object parameters)> commands,
            int commandTimeout = 30
        )
        {
            var totalRowsAffected = 0;
            await using var connection = CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                foreach (var (sql, parameters) in commands)
                {
                    totalRowsAffected += await connection.ExecuteAsync(
                        sql,
                        parameters,
                        transaction,
                        commandTimeout: commandTimeout
                    );
                }
                await transaction.CommitAsync();
                _logger.LogInformation("SQL Server 事务执行成功，共影响 {Rows} 行。", totalRowsAffected);
                return totalRowsAffected;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "SQL Server 事务执行失败，已回滚。");
                throw;
            }
        }

        #endregion

        /// <summary>
        /// 为 SqlServer 的 LIKE 查询处理特殊字符。
        /// Sqlserver 直接在字符串字面量中使用反斜杠 (\) 进行转义。
        /// </summary>
        /// <param name="source">原始搜索字符串。</param>
        /// <returns>转义后的、可用于 LIKE 子句的字符串。</returns>
        public string HandleLikeKey(string? source)
        {
            if (string.IsNullOrWhiteSpace(source)) return "%";

            // SQL Server 标准的转义方式是使用方括号[]将通配符括起来。
            // 这种方式不需要在 SQL 语句中添加 ESCAPE 子句。
            var escaped = source
                .Replace("[", "[[]") // 首先转义'['字符本身
                .Replace("%", "[%]")
                .Replace("_", "[_]");

            return $"%{escaped}%";
        }
    }
}
