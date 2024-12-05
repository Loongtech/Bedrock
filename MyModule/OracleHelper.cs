using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.LoongTech.OmniCoreX
{
    /// <summary>
    /// ORACLE 数据库操作类
    /// </summary>
    public class OracleHelper
    {
        //连接字符串
        private readonly string _connectionString;
        //ORACLE 数据库连接对象
        private OracleConnection? _connection = null;

        public OracleHelper(string connectionString)
        {
            this._connectionString = connectionString;

        }
        public OracleHelper()
        {
            _connectionString = new ConfigHelper().OracleConnString;

        }


        /// <summary>  
        /// 执行数据库非查询操作,返回受影响的行数  
        /// </summary>  
        /// <param name="cmdType">命令的类型</param>
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        /// <returns>当前查询操作影响的数据行数</returns>  
        public int ExecuteNonQuery(CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {

            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, null, cmdType, cmdText, cmdParms);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>  
        /// 执行数据库事务非查询操作,返回受影响的行数  
        /// </summary>  
        /// <param name="trans">数据库事务对象</param>  
        /// <param name="cmdType">Command类型</param>  
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        /// <returns>当前事务查询操作影响的数据行数</returns>  
        public int ExecuteNonQuery(OracleTransaction trans, CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {
            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, trans, cmdType, cmdText, cmdParms);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>  
        /// 执行数据库非查询操作,返回受影响的行数  
        /// </summary>  
        /// <param name="cmdType">Command类型</param>  
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        /// <returns>当前查询操作影响的数据行数</returns>  
        public int ExecuteNonQuery(OracleConnection connection, CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {

            OracleCommand cmd = new OracleCommand();
            PrepareCommand(cmd, null, cmdType, cmdText, cmdParms);
            int val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>  
        /// 执行数据库查询操作,返回OracleDataReader类型的内存结果集  
        /// </summary>  
        /// <param name="cmdType">命令的类型</param>
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        /// <returns>当前查询操作返回的OracleDataReader类型的内存结果集</returns>  
        public OracleDataReader ExecuteReader(CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                PrepareCommand(cmd, null, cmdType, cmdText, cmdParms);
                OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return reader;
            }
            catch
            {
                cmd.Dispose();
                _connection.Close();
                throw;
            }
        }

        /// <summary>
        /// 执行数据库查询操作,返回OracleDataReader类型的内存结果集  
        /// </summary>
        /// <param name="SqlText">SQL查询语句</param>
        /// <returns></returns>
        public OracleDataReader ExecuteReader(string SqlText)
        {
            OracleCommand cmd = new OracleCommand();
            try
            {
                PrepareCommand(cmd, null, CommandType.Text, SqlText, null);
                OracleDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return reader;
            }
            catch
            {
                cmd.Dispose();
                _connection.Close();
                throw;
            }
        }

        /// <summary>  
        /// 执行数据库查询操作,返回DataSet类型的结果集  
        /// </summary>  
        /// <param name="cmdType">命令的类型</param>
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        /// <returns>当前查询操作返回的DataSet类型的结果集</returns>  
        public DataSet ExecuteDataSet(CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {
            OracleCommand cmd = new OracleCommand();
            DataSet ds = null;
            try
            {
                PrepareCommand(cmd, null, cmdType, cmdText, cmdParms);
                using (OracleDataAdapter adapter = new OracleDataAdapter())
                {
                    adapter.SelectCommand = cmd;
                    ds = new DataSet();
                    adapter.Fill(ds);
                }
                cmd.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                _connection.Close();
                _connection.Dispose();
            }

            return ds;
        }

        /// <summary>  
        /// 执行数据库查询操作,返回DataSet类型的结果集  
        /// </summary>  
        /// <param name="sqlText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <returns>当前查询操作返回的DataSet类型的结果集</returns>  
        public DataSet ExecuteDataSet(string sqlText)
        {
            OracleCommand cmd = new OracleCommand();
            DataSet ds = null;
            try
            {
                PrepareCommand(cmd, null, CommandType.Text, sqlText, null);
                using (OracleDataAdapter adapter = new OracleDataAdapter())
                {
                    adapter.SelectCommand = cmd;
                    ds = new DataSet();
                    adapter.Fill(ds);
                }
                cmd.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                _connection.Close();
                _connection.Dispose();
            }

            return ds;
        }


        /// <summary>  
        /// 执行数据库查询操作,返回DataTable类型的结果集  
        /// </summary>  
        /// <param name="cmdType">命令的类型</param>
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        /// <returns>当前查询操作返回的DataTable类型的结果集</returns>  
        public DataTable ExecuteDataTable(CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {
            OracleCommand cmd = new OracleCommand();
            DataTable dt = null;
            try
            {
                PrepareCommand(cmd, null, cmdType, cmdText, cmdParms);
                using (OracleDataAdapter adapter = new OracleDataAdapter())
                {
                    adapter.SelectCommand = cmd;
                    dt = new DataTable();
                    adapter.Fill(dt);
                }
                cmd.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                _connection.Close();
                _connection.Dispose();
            }

            return dt;
        }


        /// <summary>  
        /// 执行数据库查询操作,返回DataTable类型的结果集  
        /// </summary>  
        /// <param name="sqlText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <returns>当前查询操作返回的DataTable类型的结果集</returns>  
        public DataTable ExecuteDataTable(string sqlText)
        {
            OracleCommand cmd = new OracleCommand();
            DataTable dt = null;

            try
            {
                PrepareCommand(cmd, null, CommandType.Text, sqlText, null);
                using (OracleDataAdapter adapter = new OracleDataAdapter())
                {
                    adapter.SelectCommand = cmd;
                    dt = new DataTable();
                    adapter.Fill(dt);
                }
                cmd.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                _connection.Close();
                _connection.Dispose();
            }

            return dt;
        }

        /// <summary>  
        /// 执行数据库查询操作,返回结果集中位于第一行第一列的Object类型的值  
        /// </summary>  
        /// <param name="cmdType">命令的类型</param>
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        /// <returns>当前查询操作返回的结果集中位于第一行第一列的Object类型的值</returns>  
        public object ExecuteScalar(CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {
            OracleCommand cmd = new OracleCommand();
            object result = null;
            try
            {
                PrepareCommand(cmd, null, cmdType, cmdText, cmdParms);
                result = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                _connection.Close();
                _connection.Dispose();
            }

            return result;
        }

        ///    <summary>  
        ///    执行数据库事务查询操作,返回结果集中位于第一行第一列的Object类型的值  
        ///    </summary>  
        ///    <param name="trans">一个已存在的数据库事务对象</param>  
        ///    <param name="cmdType">命令类型</param>  
        ///    <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        ///    <param name="cmdParms">命令参数集合</param>  
        ///    <returns>当前事务查询操作返回的结果集中位于第一行第一列的Object类型的值</returns>  
        public object ExecuteScalar(OracleTransaction trans, CommandType cmdType, string cmdText, params OracleParameter[] cmdParms)
        {
            if (trans == null)
                throw new ArgumentNullException("当前数据库事务不存在");
            _connection = trans.Connection;
            if (_connection == null)
                throw new ArgumentException("当前事务所在的数据库连接不存在");

            OracleCommand cmd = new OracleCommand();
            object result = null;

            try
            {
                PrepareCommand(cmd, trans, cmdType, cmdText, cmdParms);
                result = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
            }
            catch
            {
                throw;
            }
            finally
            {
                trans.Dispose();
                cmd.Dispose();
                _connection.Close();
                _connection.Dispose();
            }

            return result;
        }


        /// <summary>  
        /// 执行数据库命令前的准备工作  
        /// </summary>  
        /// <param name="cmd">Command对象</param>  
        /// <param name="trans">事务对象</param>  
        /// <param name="cmdType">Command类型</param>  
        /// <param name="cmdText">Oracle存储过程名称或PL/SQL命令</param>  
        /// <param name="cmdParms">命令参数集合</param>  
        private void PrepareCommand(OracleCommand cmd, OracleTransaction trans, CommandType cmdType, string cmdText, OracleParameter[] cmdParms)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                getOracleConnetion();

            cmd.Connection = _connection;
            cmd.BindByName = true;  //按照参数名称对应传值
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (OracleParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }


        /// <summary>
        /// 连接到数据库,
        /// 如果无法连接就重试
        /// </summary>
        private void getOracleConnetion()
        {

            ConfigHelper configHelper = new ConfigHelper();
            int maxRetries = configHelper.MaxRetries; //重试次数
            int RetryInterval = configHelper.RetryInterval;//重试间隔

            TimeSpan retryInterval = TimeSpan.FromMinutes(RetryInterval); //重试间隔

            if (_connection == null || _connection.State != ConnectionState.Open)
                _connection = new OracleConnection(_connectionString);
            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    _connection.Open();
                    return; // 连接成功，返回
                }
                catch
                {
                    new LogHelper().SendEvent("OracleHelper", $"数据库连接失败,等待 {RetryInterval} 分钟重新连接!", true);
                    // 连接失败，等待一段时间后重试
                    Thread.Sleep(retryInterval);
                }
            }

            // 执行到这里表示重试失败，抛出异常
            throw new Exception($"经过 {maxRetries} 次重试(每次间隔 {retryInterval.TotalMinutes} 分钟),仍然无法连接到 Oracle 数据库!!!");

        }

        /// <summary>  
        /// 将.NET日期时间类型转化为Oracle兼容的日期时间格式字符串  
        /// </summary>  
        /// <param name="date">.NET日期时间类型对象</param>  
        /// <returns>Oracle兼容的日期时间格式字符串（如该字符串：TO_DATE('2007-12-1','YYYY-MM-DD')）</returns>  
        public string GetOracleDateFormat(DateTime date)
        {
            return "TO_DATE('" + date.ToString("yyyy-M-dd") + "','YYYY-MM-DD')";
        }

        /// <summary>  
        /// 将.NET日期时间类型转化为Oracle兼容的日期时间格式字符串  
        /// </summary>  
        /// <param name="date">.NET日期时间类型对象</param>  
        /// <param name="format">Oracle日期时间类型格式化限定符</param>  
        /// <returns>Oracle兼容的日期时间格式字符串（如该字符串：TO_DATE('2007-12-01 00:00:00','yyyy-MM-dd HH24:mi:ss')）</returns>  
        public string GetOracleDateTimeFormat(DateTime date, string format)
        {
            if (format == null || format.Trim() == "") format = "YYYY-MM-DD";
            return "TO_DATE('" + date.ToString("yyyy-MM-dd HH:mm:ss") + "','" + format + "')";
        }

        /// <summary>  
        /// 将指定的关键字处理为模糊查询时的合法参数值  
        /// </summary>  
        /// <param name="source">待处理的查询关键字</param>  
        /// <returns>过滤后的查询关键字</returns>  
        public string HandleLikeKey(string source)
        {
            if (source == null || source.Trim() == "") return null;

            source = source.Replace("[", "[]]");
            source = source.Replace("_", "[_]");
            source = source.Replace("%", "[%]");

            return ("%" + source + "%");
        }

        /// <summary>
        /// 判断日期是否为节假日
        /// </summary>
        /// <param name="_dateTime">需要检查的日期</param>
        /// <returns>是/否</returns>
        public bool IsHoliday(DateTime _dateTime)
        {
            try
            {
                //检查是否是周末
                if (_dateTime.Date.DayOfWeek == DayOfWeek.Saturday || _dateTime.Date.DayOfWeek == DayOfWeek.Sunday)
                    return true;

                //根据 节假日(t_ref106)表来判断是否是 节假日
                string querySQL =
                $@"
                    SELECT SEQ
                    FROM T_REF106
                    WHERE F002 = 1
                        AND F001 = {GetOracleDateFormat(_dateTime)}
                ";
                var row = ExecuteScalar(System.Data.CommandType.Text, querySQL, null);
                if (null != row && !string.IsNullOrWhiteSpace(row.ToString()))
                    return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"[IsHoliday] 出错 -> {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// 创建OracleParameter参数列表
        /// </summary>
        /// <param name="obj">数据库对应字段对象</param>
        /// <returns></returns>
        public List<OracleParameter> CreateOracleParameters(object obj)
        {
            // 创建OracleParameter参数列表
            List<OracleParameter> parameters = new List<OracleParameter>();
            // 获取对象的所有属性
            var properties = obj.GetType().GetProperties();

            // 遍历属性
            foreach (var prop in properties)
            {
                // 获取属性的值
                var value = prop.GetValue(obj);


                // 获取OracleDbType
                OracleDbType oracleDbType = GetOracleDbType(prop.PropertyType);

                // 添加OracleParameter参数
                parameters.Add(new OracleParameter()
                {
                    // 设置参数名（转换为大写）
                    ParameterName = prop.Name.ToUpper(),
                    // 设置OracleDbType
                    OracleDbType = oracleDbType,
                    // 设置值
                    Value = (value ?? DBNull.Value),
                    // 设置方向
                    Direction = ParameterDirection.Input
                });
            }

            // 返回参数列表
            return parameters;

        }

        // 根据类型，获取OracleDbType
        private static OracleDbType GetOracleDbType(Type type)
        {
            // 如果类型是string，则返回OracleDbType.Varchar2
            if (type == typeof(string))
            {
                return OracleDbType.Varchar2;
            }
            // 如果类型是DateTime或DateTime?，则返回OracleDbType.Date
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return OracleDbType.Date;
            }
            // 如果类型是int或int?，则返回OracleDbType.Int32
            if (type == typeof(int) || type == typeof(int?))
            {
                return OracleDbType.Int32;
            }
            // 如果类型是long或long?，则返回OracleDbType.Int64
            if (type == typeof(long) || type == typeof(long?))
            {
                return OracleDbType.Int64;
            }
            // 如果类型是decimal或decimal?，则返回OracleDbType.Decimal
            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return OracleDbType.Decimal;
            }
            if (type == typeof(short) || type == typeof(short?))
            {
                return OracleDbType.Int16;
            }
            if (type == typeof(double) || type == typeof(double?))
            {
                return OracleDbType.Double;
            }
            // 添加更多的类型映射，如果需要的话
            // 如果类型不是以上任何一种，则抛出NotSupportedException
            throw new NotSupportedException($"Type {type.Name} is not supported.");
        }
    }
}
