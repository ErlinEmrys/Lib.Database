using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// Basic database connection interface
    /// </summary>
    public interface IDbConnect : IDisposable
    {
        /// <summary>
        /// Default timeout for every command (seconds)
        /// </summary>
        public const int DEFAULT_COMMAND_TIMEOUT_SECONDS = 500;

        /// <summary>
        /// Open connection to database
        /// </summary>
        void Open();

        /// <summary>
        /// Close connection to database
        /// </summary>
        void Close();

        /// <summary>
        /// Begin new database transaction
        /// </summary>
        /// <returns>Begined transaction</returns>
        IDbTransaction BeginTransaction();

        /// <summary>
        /// Commit current active database transaction
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Make rollback of current active database transaction
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Returns dataset from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        DataSet GetDataSet(string query, List<SqlParam>? prms = null);

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        DataSet GetDataSetSp(string sp, List<SqlParam>? prms = null);

        /// <summary>
        /// Executes SQL command without return value
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        void Execute(string query, List<SqlParam>? prms = null);

        /// <summary>
        /// Executes SQL stored procedure without return value
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        void ExecuteSp(string sp, List<SqlParam>? prms = null);

        /// <summary>
        /// Returns reader from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Reader</returns>
        IDbDataReader GetDataReader(string query, List<SqlParam>? prms = null);

        /// <summary>
        /// Returns reader from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>reader</returns>
        IDbDataReader GetDataReaderSp(string sp, List<SqlParam>? prms = null);
    }
}