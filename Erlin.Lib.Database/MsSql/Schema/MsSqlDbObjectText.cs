using System;
using System.Collections.Generic;
using System.Text;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.MsSql.Schema
{
    /// <summary>
    /// Microsoft SQL databse object text
    /// </summary>
    public class MsSqlDbObjectText : MsSqlDbObjectBase
    {
        /// <summary>
        /// Query for select query texts from db
        /// </summary>
        public static string SelectQuery { get; } = @$"
SELECT sch.name as [{nameof(SchemaName)}],
        so.name as [{nameof(ObjectName)}],
        so.[type] AS [{nameof(ObjectType)}],
        sc.[colid] AS [{nameof(OrderId)}],
        sc.[text] AS [{nameof(Text)}]
FROM sys.sysobjects so WITH(NOLOCK)
JOIN sys.syscomments sc WITH(NOLOCK) ON so.id = sc.id
JOIN sys.schemas sch WITH(NOLOCK) ON so.[uid] = sch.[schema_id] AND sch.name != 'sys'
ORDER BY [{nameof(SchemaName)}], [{nameof(ObjectName)}]
";

        /// <summary>
        /// Order id of this parameter
        /// </summary>
        public short OrderId { get; protected set; }

        /// <summary>
        /// Query text
        /// </summary>
        public string Text { get; protected set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public MsSqlDbObjectText()
        {
            Text = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// Read data from db result
        /// </summary>
        /// <param name="reader">DB reader</param>
        public override void DbRead(IDbDataReader reader)
        {
            base.DbRead(reader);

            OrderId = reader.ReadInt16(nameof(OrderId));
            Text = reader.ReadString(nameof(Text));
        }
    }
}
