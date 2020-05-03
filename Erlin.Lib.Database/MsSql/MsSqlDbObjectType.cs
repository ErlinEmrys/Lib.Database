using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erlin.Lib.Database.MsSql
{
    /// <summary>
    /// Types of database object for Ms-SQL databases
    /// </summary>
    public enum MsSqlDbObjectType
    {
        /// <summary>
        /// Unknow type or error
        /// </summary>
        Error = 0,
        /// <summary>
        /// Aggregate function (CLR)
        /// </summary>
        AF = 1,
        /// <summary>
        /// CHECK constraint
        /// </summary>
        C = 2,
        /// <summary>
        /// DEFAULT (constraint or stand-alone)
        /// </summary>
        D = 3,
        /// <summary>
        /// FOREIGN KEY constraint
        /// </summary>
        F = 4,
        /// <summary>
        /// SQL scalar function
        /// </summary>
        FN = 5,
        /// <summary>
        /// Assembly (CLR) scalar-function
        /// </summary>
        FS = 6,
        /// <summary>
        /// Assembly (CLR) table-valued function
        /// </summary>
        FT = 7,
        /// <summary>
        /// SQL inline table-valued function
        /// </summary>
        IF = 8,
        /// <summary>
        /// Internal table
        /// </summary>
        IT = 9,
        /// <summary>
        /// SQL Stored Procedure
        /// </summary>
        P = 10,
        /// <summary>
        /// Assembly (CLR) stored-procedure
        /// </summary>
        PC = 11,
        /// <summary>
        /// Plan guide
        /// </summary>
        PG = 12,
        /// <summary>
        /// PRIMARY KEY constraint
        /// </summary>
        PK = 13,
        /// <summary>
        /// Rule (old-style, stand-alone)
        /// </summary>
        R = 14,
        /// <summary>
        /// Replication-filter-procedure
        /// </summary>
        RF = 15,
        /// <summary>
        /// System base table
        /// </summary>
        S = 16,
        /// <summary>
        /// Synonym
        /// </summary>
        SN = 17,
        /// <summary>
        /// Service queue
        /// </summary>
        SQ = 18,
        /// <summary>
        /// Assembly (CLR) DML trigger
        /// </summary>
        TA = 19,
        /// <summary>
        /// SQL table-valued-function
        /// </summary>
        TF = 20,
        /// <summary>
        /// SQL DML trigger
        /// </summary>
        TR = 21,
        /// <summary>
        /// Table type
        /// </summary>
        TT = 22,
        /// <summary>
        /// Table (user-defined)
        /// </summary>
        U = 23,
        /// <summary>
        /// UNIQUE constraint
        /// </summary>
        UQ = 24,
        /// <summary>
        /// View
        /// </summary>
        V = 25,
        /// <summary>
        /// Extended stored procedure
        /// </summary>
        X = 26,
    }
}