using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Net.LoongTech.Bedrock.Data
{
    /// <summary>
    /// Dapper 自定义参数类，用于显式地将 byte[] 绑定为 Oracle 的 BLOB 类型。
    /// 这可以解决 ODP.NET 驱动在处理大字节数组时可能发生的 ORA-01460 错误。
    /// </summary>
    public class OracleBlobParameter : SqlMapper.ICustomQueryParameter
    {
        /// <summary>
        /// 存储二进制数据的私有字段
        /// </summary>
        private readonly byte[] _value;

        /// <summary>
        /// 构造函数，接收需要作为 BLOB 存储的字节数组
        /// </summary>
        /// <param name="value">要传入的二进制数据</param>
        public OracleBlobParameter(byte[] value)
        {
            _value = value;
        }

        /// <summary>
        /// Dapper 在构建 SQL 命令时会调用此方法来添加自定义参数。
        /// </summary>
        /// <param name="command">当前的数据库命令对象，Dapper 在此基础上构建。</param>
        /// <param name="name">Dapper 从匿名对象或动态参数中解析出的参数名称。</param>
        public void AddParameter(IDbCommand command, string name)
        {
            // 1. 创建一个 Oracle 特定的参数对象
            // Dapper 的 IDbCommand 是一个通用接口，需要将其转换为具体的 OracleCommand 才能设置 Oracle 特有的属性。
            var oracleCommand = (OracleCommand)command;
            var parameter = oracleCommand.CreateParameter();

            // 2. 配置该参数
            parameter.ParameterName = name; // 设置参数名，例如 :Content
            parameter.OracleDbType = OracleDbType.Blob; // <-- 这是解决问题的核心：显式指定参数类型为 BLOB
            parameter.Direction = ParameterDirection.Input; // 指定这是一个输入参数
            parameter.Value = _value; // 将构造函数中传入的 byte[] 赋值给参数

            // 3. 将配置好的参数添加到命令的参数集合中
            command.Parameters.Add(parameter);
        }
    }
}
