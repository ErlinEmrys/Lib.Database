using System;
using System.Collections.Generic;
using System.Text;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// United type of database types
    /// </summary>
    public enum SqlParamType
    {
        /// <summary>
        /// Error
        /// </summary>
        Error = 0,
        /// <summary>
        /// Boolean
        /// </summary>
        Bool = 1,
        /// <summary>
        /// Varchar
        /// </summary>
        StringAnsi = 2,
        /// <summary>
        /// NVarcahar
        /// </summary>
        StringUtf8 = 3,
        /// <summary>
        /// DateTime
        /// </summary>
        DateTime = 4,
        /// <summary>
        /// Date
        /// </summary>
        Date = 5,
        /// <summary>
        /// Time
        /// </summary>
        Time = 6,
        /// <summary>
        /// Short
        /// </summary>
        Int16 = 7,
        /// <summary>
        /// Int
        /// </summary>
        Int32 = 8,
        /// <summary>
        /// Long
        /// </summary>
        Int64 = 9,
        /// <summary>
        /// Decimal
        /// </summary>
        Decimal = 10,
    }
}