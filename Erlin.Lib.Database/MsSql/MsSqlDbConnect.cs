using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Exceptions;
using Erlin.Lib.Database.MsSql.Schema;

using Microsoft.Data.SqlClient;

namespace Erlin.Lib.Database.MsSql
{
    /// <summary>
    /// Microsoft SQL database connection object
    /// </summary>
    public sealed class MsSqlDbConnect : IDbConnect
    {
        private readonly SqlConnection _connection;
        private readonly string _connectionString;
        private SqlTransaction? _transaction;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connection">Existing connection object</param>
        public MsSqlDbConnect(SqlConnection connection)
        {
            _connection = connection;
            _connectionString = _connection.ConnectionString;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString">MS-SQL connection string</param>
        public MsSqlDbConnect(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connection = new SqlConnection(connectionString);
            _connectionString = connectionString;
        }

        /// <summary>
        /// Open connection to database
        /// </summary>
        public void Open()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// Close connection to database
        /// </summary>
        public void Close()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
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

            _transaction = _connection.BeginTransaction();
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
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }

        /// <summary>
        /// Read complete database schema
        /// </summary>
        /// <returns>Parsed database schema</returns>
        public MsSqlDbSchema ReadSchema()
        {
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            MsSqlDbSchema result = new MsSqlDbSchema(connStringBuilder.DataSource, connStringBuilder.InitialCatalog);
            result.ReadSchema(this);
            return result;
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

            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                da.SelectCommand = CreateSqlCommand(commandText, commandType, prms);
                da.Fill(result);
            }

            return result;
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
            SqlCommand command = CreateSqlCommand(commandText, commandType, prms);
            return (T)command.ExecuteScalar();
        }

        /// <summary>
        /// Returns reader from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Reader</returns>
        public MsSqlDataReader GetDataReader(string query, List<SqlParam>? prms = null)
        {
            return GetDataReaderImpl(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Returns reader from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>reader</returns>
        public MsSqlDataReader GetDataReaderSp(string sp, List<SqlParam>? prms = null)
        {
            return GetDataReaderImpl(sp, CommandType.StoredProcedure, prms);
        }

        /// <summary>
        /// Returns data reader from SQL command
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandType">Command type</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Result data reader</returns>
        private MsSqlDataReader GetDataReaderImpl(string commandText, CommandType commandType, List<SqlParam>? prms = null)
        {
            SqlCommand command = CreateSqlCommand(commandText, commandType, prms);
            return new MsSqlDataReader(command.ExecuteReader());
        }

        /// <summary>
        /// Creates new sql command
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandType">Command type</param>
        /// <param name="prms">Command parameters</param>
        /// <param name="commandTimeOut">Command timeout</param>
        /// <returns>Sql command</returns>
        private SqlCommand CreateSqlCommand(string commandText, CommandType commandType, List<SqlParam>? prms = null, int? commandTimeOut = null)
        {
            SqlCommand result = new SqlCommand();
            result.Connection = _connection;
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
        private static void AddParamToCommand(SqlCommand command, List<SqlParam>? prms = null)
        {
            if (prms != null)
            {
                foreach (SqlParam fParam in prms)
                {
                    object value = SimpleConvert.ConvertNullToDbNull(fParam.Value);
                    SqlParameter newpar = command.Parameters.AddWithValue(fParam.SqlName, value);
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

                    newpar.SqlDbType = ConvertDatabaseType(fParam.SqlType);

                    if (fParam.Size.HasValue)
                    {
                        newpar.Size = fParam.Size.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Converts unified sql db type to MS-SQL Db type
        /// </summary>
        /// <param name="unitedType">Unified db type</param>
        /// <returns>MS-SQL Db type</returns>
        private static SqlDbType ConvertDatabaseType(SqlParamType unitedType)
        {
            switch (unitedType)
            {
                case SqlParamType.Decimal:
                    return SqlDbType.Decimal;
                case SqlParamType.Int16:
                    return SqlDbType.SmallInt;
                case SqlParamType.Int32:
                    return SqlDbType.Int;
                case SqlParamType.Int64:
                    return SqlDbType.BigInt;
                case SqlParamType.StringUtf8:
                    return SqlDbType.NVarChar;
                case SqlParamType.StringAnsi:
                    return SqlDbType.VarChar;
                case SqlParamType.DateTime:
                    return SqlDbType.DateTime;
                case SqlParamType.Date:
                    return SqlDbType.Date;
                case SqlParamType.Time:
                    return SqlDbType.Time;
                case SqlParamType.Bool:
                    return SqlDbType.Bit;
                default:
                    throw new EnumValueNotImplementedException(unitedType);
            }
        }
    }
}