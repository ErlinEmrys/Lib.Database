using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Erlin.Lib.Database.Schema;

using Npgsql;

namespace Erlin.Lib.Database.PgSql
{
    /// <summary>
    /// PostgreSql DB connection
    /// </summary>
    public sealed class PgSqlDbConnect : IDbConnect
    {
        private NpgsqlConnection _connection;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString">PgSql connection string</param>
        public PgSqlDbConnect(string connectionString)
        {
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
        /// Read complete database schema
        /// </summary>
        /// <returns>Parsed database schema</returns>
        public DbObjectCatalogSchema ReadSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns dataset from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public DataSet GetDataSetQuery(string query, List<SqlParam> prms)
        {
            throw new NotImplementedException();
        }
    }
}
