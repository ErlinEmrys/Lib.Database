using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Type of comparison result for database schemas
    /// </summary>
    [Flags]
    public enum DbSchemaCompareResultType
    {
        /// <summary>
        /// Objects are equal
        /// </summary>
        Equals = 0,
        /// <summary>
        /// Object in checked database is missing
        /// </summary>
        Missing = 1,
        /// <summary>
        /// Objects are different
        /// </summary>
        Different = 2,
        /// <summary>
        /// Object in checked database is redudant
        /// </summary>
        Redudant = 4,
    }
}