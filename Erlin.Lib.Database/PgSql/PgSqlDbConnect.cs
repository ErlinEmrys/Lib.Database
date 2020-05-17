using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

using Erlin.Lib.Common;
using Erlin.Lib.Database.PgSql.Schema;
using Erlin.Lib.Database.Schema;

using Npgsql;

namespace Erlin.Lib.Database.PgSql
{
    /// <summary>
    /// PostgreSql DB connection
    /// </summary>
    public sealed class PgSqlDbConnect : IDbConnect
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlConnectionStringBuilder _connectionString;
        private NpgsqlTransaction? _transaction;

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

            _connection = new NpgsqlConnection(connectionString);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _connection.Dispose();
        }

        /// <summary>
        /// Open connection to database
        /// </summary>
        public void Open()
        {
            _connection.Open();
        }

        /// <summary>
        /// Close connection to database
        /// </summary>
        public void Close()
        {
            _connection.Close();
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
        /// Read complete database schema
        /// </summary>
        /// <returns>Parsed database schema</returns>
        public PgSqlDbSchema ReadSchema()
        {
            PgSqlDbSchema result = new PgSqlDbSchema(_connectionString.Host!, _connectionString.Database!);
            result.ReadSchema(this);
            return result;
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

                    if (fParam.SqlType.HasValue)
                    {
                        newpar.DbType = fParam.SqlType.Value;
                    }

                    if (fParam.Size.HasValue)
                    {
                        newpar.Size = fParam.Size.Value;
                    }
                }
            }
        }
    }
}
