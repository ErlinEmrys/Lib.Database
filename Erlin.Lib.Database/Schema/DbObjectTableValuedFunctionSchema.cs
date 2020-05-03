using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Schema of database table valued function (function that returns table)
    /// </summary>
    [DebuggerDisplay("TFN: [{DbSchemaName}].[{Name}]")]
    public class DbObjectTableValuedFunctionSchema : DbObjectFunctionSchema
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbObjectId">Databse object ID</param>
        /// <param name="dbSchemaName">Databse schema name</param>
        public DbObjectTableValuedFunctionSchema(string name, long dbObjectId, string dbSchemaName) : base(name, dbObjectId, dbSchemaName)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectTableValuedFunctionSchema()
        {
        }
    }
}