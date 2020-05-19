using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Exceptions;
using Erlin.Lib.Common.Time;
using Erlin.Lib.Database.PgSql.Schema;
using Erlin.Lib.Database.Schema;

using Npgsql;

using NpgsqlTypes;

namespace Erlin.Lib.Database.PgSql
{
    /// <summary>
    /// PostgreSql DB connection
    /// </summary>
    public sealed class PgSqlDbConnect : IDbConnect
    {
        private const string SMP_GENESIS_REQUEST = "spm_genesis_request";
        private const string SMP_READ_FILE = "spm_read_file_utf8";

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

        private readonly NpgsqlConnectionStringBuilder _connectionString;
        private NpgsqlTransaction? _transaction;

        /// <summary>
        /// Default query timeout
        /// </summary>
        public int DefaultTimeout { get; private set; } = 30 * 1000;

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
            _connectionString = new NpgsqlConnectionStringBuilder(connectionString);
            if (string.IsNullOrEmpty(_connectionString.Host))
            {
                throw new InvalidOperationException($"Connection string {_connectionString} does not contains Host!");
            }
            if (string.IsNullOrEmpty(_connectionString.Database))
            {
                throw new InvalidOperationException($"Connection string {_connectionString} does not contains Database!");
            }

            UnderlyingConnection = new NpgsqlConnection(connectionString);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            UnderlyingConnection.Dispose();
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
        /// Read complete database schema
        /// </summary>
        /// <returns>Parsed database schema</returns>
        public byte[] ReadSchema()
        {
            string token = $"{Guid.NewGuid()}";
            string? filePath = null;

            UnderlyingConnection.Notification += delegate(object o, NpgsqlNotificationEventArgs args)
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
                                        };
            //Request the creation of file
            ExecuteSp(SMP_GENESIS_REQUEST, new List<SqlParam> { new SqlParam("command", 0, SqlParamType.Int32), new SqlParam("token", token, SqlParamType.NVarchar) });
            if (!UnderlyingConnection.Wait(DefaultTimeout))
            {
                throw new TimeoutException("CHANNEL_GENESIS_RESPOND");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new TimeoutException("FILE_PATH_EMPTY");
            }

            //Read the file
            byte[] data = ExecuteSp<byte[]>(SMP_READ_FILE, new List<SqlParam> { new SqlParam("path", filePath, SqlParamType.NVarchar) });

            //Delete the file
            ExecuteSp(SMP_GENESIS_REQUEST,
                      new List<SqlParam> { new SqlParam("command", 1, SqlParamType.Int32), new SqlParam("token", filePath, SqlParamType.NVarchar) });

            return data;
        }

        /// <summary>
        /// Returns dataset from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public DataSet GetDataSetQuery(string query, List<SqlParam> prms)
        {
            return GetDataSetImpl(query, CommandType.Text, prms);
        }

        private DataSet GetDataSetImpl(string commandText, CommandType commandType, List<SqlParam>? prms)
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
        /// Returns reader from SQL stored procedure
        /// </summary>
        /// <param name="query">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>reader</returns>
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

        private PgSqlDataReader GetDataReaderImpl(string commandText, CommandType commandType, List<SqlParam>? prms)
        {
            NpgsqlCommand command = CreateSqlCommand(commandText, commandType, prms);
            return new PgSqlDataReader(command.ExecuteReader());
        }

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="query">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public void Execute(string query, List<SqlParam>? prms = null)
        {
            ExecuteImpl<object>(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="query">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public T Execute<T>(string query, List<SqlParam>? prms = null)
        {
            return ExecuteImpl<T>(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public void ExecuteSp(string sp, List<SqlParam>? prms = null)
        {
            ExecuteImpl<object>(sp, CommandType.StoredProcedure, prms);
        }

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public T ExecuteSp<T>(string sp, List<SqlParam>? prms = null)
        {
            return ExecuteImpl<T>(sp, CommandType.StoredProcedure, prms);
        }

        private T ExecuteImpl<T>(string commandText, CommandType commandType, List<SqlParam>? prms)
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
        private NpgsqlCommand CreateSqlCommand(string commandText, CommandType commandType, List<SqlParam>? prms, int? commandTimeOut = null)
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

        private static void AddParamToCommand(NpgsqlCommand command, List<SqlParam>? prms)
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

        private static NpgsqlDbType ConvertDatabaseType(SqlParamType unitedType)
        {
            switch (unitedType)
            {
                case SqlParamType.Int32:
                    return NpgsqlDbType.Integer;
                case SqlParamType.NVarchar:
                case SqlParamType.Varchar:
                    return NpgsqlDbType.Varchar;
                default:
                    throw new EnumValueNotImplementedException(unitedType);
            }
        }
    }
}
