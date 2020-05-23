using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Exceptions;

using Npgsql;

using NpgsqlTypes;

namespace Erlin.Lib.Database.PgSql
{
    /// <summary>
    /// PostgreSql DB connection
    /// </summary>
    public sealed class PgSqlDbConnect : IDbConnect
    {
        private const string SPM_GENESIS_REQUEST = "spm_genesis_request";
        private const string SPM_READ_FILE = "spm_read_file_utf8";

        /// <summary>
        /// Channel name for pgenesis requests
        /// </summary>
        public const string CHANNEL_GENESIS_REQUEST = "genesis_request";

        /// <summary>
        /// Channel name for pgenesis respond
        /// </summary>
        public const string CHANNEL_GENESIS_RESPOND = "genesis_respond";

        /// <summary>
        /// Channel name for pgenesis delete
        /// </summary>
        public const string CHANNEL_GENESIS_DELETE = "genesis_delete";

        /// <summary>
        /// Channel name for pgenesis backup requests
        /// </summary>
        public const string CHANNEL_BACKUP_REQUEST = "backup_request";

        private NpgsqlTransaction? _transaction;

        /// <summary>
        /// Default query timeout
        /// </summary>
        public int DefaultTimeout { get; set; } = 30 * 1000;

        /// <summary>
        /// Underlying connection object
        /// </summary>
        public NpgsqlConnection UnderlyingConnection { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString">PgSql connection string</param>
        public PgSqlDbConnect(string connectionString)
        {
            UnderlyingConnection = new NpgsqlConnection(connectionString);
        }

        /// <summary>
        /// Open connection to database
        /// </summary>
        public void Open()
        {
            UnderlyingConnection.Open();
        }

        /// <summary>
        /// Close connection to database
        /// </summary>
        public void Close()
        {
            UnderlyingConnection.Close();
        }

        /// <summary>
        /// Begin new database transaction
        /// </summary>
        /// <returns>Begined transaction</returns>
        public IDbTransaction BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot begin another database transaction - transaction in progress!");
            }

            _transaction = UnderlyingConnection.BeginTransaction();
            return _transaction;
        }

        /// <summary>
        /// Commit current active database transaction
        /// </summary>
        public void CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Cannot commit database transaction - transaction not exist!");
            }

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        /// <summary>
        /// Make rollback of current active database transaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Cannot rollback database transaction - transaction not exist!");
            }

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        /// <summary>
        /// Returns dataset from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public DataSet GetDataSet(string query, List<SqlParam>? prms = null)
        {
            return GetDataSetImpl(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public DataSet GetDataSetSp(string sp, List<SqlParam>? prms = null)
        {
            return GetDataSetImpl(sp, CommandType.StoredProcedure, prms);
        }

        /// <summary>
        /// Returns reader from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Reader</returns>
        IDbDataReader IDbConnect.GetDataReader(string query, List<SqlParam>? prms)
        {
            return GetDataReader(query, prms);
        }

        /// <summary>
        /// Returns reader from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>reader</returns>
        IDbDataReader IDbConnect.GetDataReaderSp(string sp, List<SqlParam>? prms)
        {
            return GetDataReaderSp(sp, prms);
        }

        /// <summary>
        /// Executes SQL command without return value
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        public void Execute(string query, List<SqlParam>? prms = null)
        {
            ExecuteImpl<object>(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Executes SQL stored procedure without return value
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        public void ExecuteSp(string sp, List<SqlParam>? prms = null)
        {
            ExecuteImpl<object>(sp, CommandType.StoredProcedure, prms);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            UnderlyingConnection.Dispose();
        }

        /// <summary>
        /// Read complete database schema
        /// </summary>
        /// <returns>Parsed database schema</returns>
        public byte[] ReadSchema()
        {
            string token = $"{Guid.NewGuid()}";
            string? filePath = null;

            void OnUnderlyingConnectionOnNotification(object o, NpgsqlNotificationEventArgs args)
            {
                try
                {
                    if (args.Channel == CHANNEL_GENESIS_RESPOND)
                    {
                        filePath = args.Payload;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }

            UnderlyingConnection.Notification += OnUnderlyingConnectionOnNotification;

            //Request the creation of file
            ExecuteSp(SPM_GENESIS_REQUEST, new List<SqlParam> { new SqlParam("command", 0, SqlParamType.Int32), new SqlParam("token", token, SqlParamType.StringUtf8) });
            if (!UnderlyingConnection.Wait(DefaultTimeout))
            {
                throw new TimeoutException("CHANNEL_GENESIS_RESPOND");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new TimeoutException("FILE_PATH_EMPTY");
            }

            //Read the file
            byte[] data = ExecuteSp<byte[]>(SPM_READ_FILE, new List<SqlParam> { new SqlParam("path", filePath, SqlParamType.StringUtf8) });

            //Delete the file
            ExecuteSp(SPM_GENESIS_REQUEST,
                      new List<SqlParam> { new SqlParam("command", 1, SqlParamType.Int32), new SqlParam("token", filePath, SqlParamType.StringUtf8) });

            UnderlyingConnection.Notification -= OnUnderlyingConnectionOnNotification;

            return data;
        }

        /// <summary>
        /// Returns dataset
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandType">Command type</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        private DataSet GetDataSetImpl(string commandText, CommandType commandType, List<SqlParam>? prms = null)
        {
            DataSet result = new DataSet();
            result.Locale = CultureInfo.InvariantCulture;

            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter())
            {
                da.SelectCommand = CreateSqlCommand(commandText, commandType, prms);
                da.Fill(result);
            }

            return result;
        }

        /// <summary>
        /// Returns reader from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Reader</returns>
        public PgSqlDataReader GetDataReader(string query, List<SqlParam>? prms = null)
        {
            return GetDataReaderImpl(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Returns reader from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>reader</returns>
        public PgSqlDataReader GetDataReaderSp(string sp, List<SqlParam>? prms = null)
        {
            return GetDataReaderImpl(sp, CommandType.StoredProcedure, prms);
        }

        /// <summary>
        /// Returns reader from SQL command
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandType">Command type</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Reader</returns>
        private PgSqlDataReader GetDataReaderImpl(string commandText, CommandType commandType, List<SqlParam>? prms = null)
        {
            NpgsqlCommand command = CreateSqlCommand(commandText, commandType, prms);
            return new PgSqlDataReader(command.ExecuteReader());
        }

        /// <summary>
        /// Executes SQL command with simple return value
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public T Execute<T>(string query, List<SqlParam>? prms = null)
        {
            return ExecuteImpl<T>(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Executes SQL stored procedure with return value
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public T ExecuteSp<T>(string sp, List<SqlParam>? prms = null)
        {
            return ExecuteImpl<T>(sp, CommandType.StoredProcedure, prms);
        }

        /// <summary>
        /// Executes SQL command with return value
        /// </summary>
        /// <typeparam name="T">Runtime type of expected result</typeparam>
        /// <param name="commandText">Command text</param>
        /// <param name="commandType">Command type</param>
        /// <param name="prms">SQL arguments</param>
        /// <returns>Result value</returns>
        private T ExecuteImpl<T>(string commandText, CommandType commandType, List<SqlParam>? prms = null)
        {
            NpgsqlCommand command = CreateSqlCommand(commandText, commandType, prms);
            return (T)command.ExecuteScalar();
        }

        /// <summary>
        /// Creates new sql command
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandType">Command type</param>
        /// <param name="prms">Command parameters</param>
        /// <param name="commandTimeOut">Command timeout</param>
        /// <returns>Sql command</returns>
        private NpgsqlCommand CreateSqlCommand(string commandText, CommandType commandType, List<SqlParam>? prms = null, int? commandTimeOut = null)
        {
            NpgsqlCommand result = new NpgsqlCommand();
            result.Connection = UnderlyingConnection;
            result.Transaction = _transaction;
            result.CommandTimeout = IDbConnect.DEFAULT_COMMAND_TIMEOUT_SECONDS;
            if (commandTimeOut.HasValue)
            {
                result.CommandTimeout = commandTimeOut.Value;
            }

            result.CommandText = commandText;
            result.CommandType = commandType;

            AddParamToCommand(result, prms);

            return result;
        }

        /// <summary>
        /// Add SQL parameter to command object
        /// </summary>
        /// <param name="command">SQL command object</param>
        /// <param name="prms">SQL parameter to add</param>
        private static void AddParamToCommand(NpgsqlCommand command, List<SqlParam>? prms = null)
        {
            if (prms != null)
            {
                foreach (SqlParam fParam in prms)
                {
                    object value = SimpleConvert.ConvertNullToDbNull(fParam.Value);
                    NpgsqlParameter newpar = command.Parameters.AddWithValue(fParam.SqlName, value);
                    if (fParam.Direction != ParameterDirection.Input)
                    {
                        newpar.Direction = fParam.Direction;

                        //For non-inputed parameter save the original parameter object to retrieve value after SQL execute
                        if (fParam.SqlParameter != null)
                        {
                            throw new InvalidOperationException("Concurency SQL execution call on non-input sql parameter! Not supported!");
                        }

                        fParam.SqlParameter = newpar;
                    }

                    newpar.NpgsqlDbType = ConvertDatabaseType(fParam.SqlType);

                    if (fParam.Size.HasValue)
                    {
                        newpar.Size = fParam.Size.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Converts unified sql db type to PostgreSql Db type
        /// </summary>
        /// <param name="unitedType">Unified db type</param>
        /// <returns>PostgreSql Db type</returns>
        private static NpgsqlDbType ConvertDatabaseType(SqlParamType unitedType)
        {
            switch (unitedType)
            {
                case SqlParamType.Decimal:
                    return NpgsqlDbType.Double;
                case SqlParamType.Int16:
                    return NpgsqlDbType.Smallint;
                case SqlParamType.Int32:
                    return NpgsqlDbType.Integer;
                case SqlParamType.Int64:
                    return NpgsqlDbType.Bigint;
                case SqlParamType.StringUtf8:
                case SqlParamType.StringAnsi:
                    return NpgsqlDbType.Varchar;
                case SqlParamType.DateTime:
                    return NpgsqlDbType.Timestamp;
                case SqlParamType.Date:
                    return NpgsqlDbType.Date;
                case SqlParamType.Time:
                    return NpgsqlDbType.Time;
                case SqlParamType.Bool:
                    return NpgsqlDbType.Bit;
                default:
                    throw new EnumValueNotImplementedException(unitedType);
            }
        }
    }
}