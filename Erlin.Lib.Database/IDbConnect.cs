﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Erlin.Lib.Database.Schema;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// Basic database connection interface
    /// </summary>
    public interface IDbConnect : IDisposable
    {
        /// <summary>
        /// Open connection to database
        /// </summary>
        void Open();

        /// <summary>
        /// Close connection to database
        /// </summary>
        void Close();

        /// <summary>
        /// Read complete database schema
        /// </summary>
        /// <returns>Parsed database schema</returns>
        DbObjectCatalogSchema ReadSchema();

        /// <summary>
        /// Returns dataset from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        DataSet GetDataSetQuery(string query, List<SqlParam> prms);
    }
}