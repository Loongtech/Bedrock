using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Net.LoongTech.Bedrock.Data
{
    /// <summary>
    /// 使用 Dapper 的现代化 MySQL 数据库操作辅助类。
    /// 实现了通用的 IDbHelper 接口。
    /// </summary>
    public class MySqlHelper : IDbHelper, IAdvancedDbHelper // <--- 实现接口
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        /// <summary>
        /// 初始化 MySqlHelper 的新实例。
        /// </summary>
        public MySqlHelper(string connectionString, ILogger logger)
        {
            _connectionString = connectionString;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 内部方法，用于创建连接
        private MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
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
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 MySQL 查询失败。SQL: {Sql}", sql);
                throw; // 重新抛出异常，让上层处理
            }
        }

        public async Task<T> QueryFirstOrDefaultAsync<T>(
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
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 MySQL 单行查询失败。SQL: {Sql}", sql);
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
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 MySQL 非查询操作失败。SQL: {Sql}", sql);
                throw;
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(
            string sql,
            object? parameters = null,
            int commandTimeout = 30
        )
        {
            try
            {
                await using var connection = CreateConnection();
                return await connection.ExecuteScalarAsync<T>(sql,
                    parameters,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 MySQL 标量查询失败。SQL: {Sql}", sql);
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

                // 1. 手动根据 reader 的结构创建 DataTable 的列
                // 这种方式只创建列名和数据类型，不包含任何约束（如主键）
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }

                // 2. 逐行读取 reader 中的数据并添加到 DataTable
                while (await reader.ReadAsync())
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        // 处理数据库中的 NULL 值
                        dataRow[i] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                    }
                    dataTable.Rows.Add(dataRow);
                }
                // ------------------------------------

                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 MySQL 查询并填充 DataTable 失败。SQL: {Sql}", sql);
                throw;
            }
        }

        #endregion

        #region IAdvancedDbHelper 实现 存储过程处理 (使用核心 Dapper)

        /// <summary>
        /// 异步执行 MySQL 存储过程并返回结果集。
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
                // Dapper 会自动处理 MySQL 的存储过程调用
                return await connection.QueryAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 MySQL 高级存储过程 {ProcedureName} (返回结果集) 失败。", procedureName);
                throw;
            }
        }

        /// <summary>
        /// 异步执行 MySQL 存储过程，通常用于包含 OUT/INOUT 参数的场景。
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
                await connection.ExecuteAsync(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "执行 MySQL 高级存储过程 {ProcedureName} (无结果集) 失败。", procedureName);
                throw;
            }
        }

        #endregion
        #region 事务处理 (更安全的方式)

        /// <summary>
        /// (推荐) 异步执行多条 SQL 语句（事务）。
        /// 这种方式比传递字符串列表更安全，因为它支持参数化查询，能防止 SQL 注入。
        /// </summary>
        /// <param name="commands">包含 SQL 语句和参数的命令元组列表。</param>
        /// <returns>总共受影响的行数。</returns>
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
                    // 将事务对象传递给 Dapper
                    totalRowsAffected += await connection.ExecuteAsync(
                        sql, parameters,
                        transaction,
                        commandTimeout: commandTimeout
                    );
                }
                await transaction.CommitAsync();
                _logger.LogInformation("MySQL 事务执行成功，共影响 {Rows} 行。", totalRowsAffected);
                return totalRowsAffected;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "MySQL 事务执行失败，已回滚。");
                throw;
            }
        }

        #endregion

        /// <summary>
        /// 为 MySQL 的 LIKE 查询处理特殊字符。
        /// MySql 直接在字符串字面量中使用反斜杠 (\) 进行转义。
        /// </summary>
        /// <param name="source">原始搜索字符串。</param>
        /// <returns>转义后的、可用于 LIKE 子句的字符串。</returns>
        public string HandleLikeKey(string? source)
        {
            if (string.IsNullOrWhiteSpace(source)) return "%";

            // MySQL 也需要在 SQL 中配合使用 ESCAPE '\'
            var escaped = source
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            return $"%{escaped}%";
        }
    }
}
