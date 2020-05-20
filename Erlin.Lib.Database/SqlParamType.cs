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
        /// Int 32
        /// </summary>
        Int32 = 1,
        /// <summary>
        /// Varchar
        /// </summary>
        Varchar = 2,
        /// <summary>
        /// NVarcahar
        /// </summary>
        NVarchar = 3,
    }
}