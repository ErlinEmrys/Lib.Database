using System;
using System.Collections.Generic;
using System.Text;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.MsSql.Schema
{
    /// <summary>
    /// Microsoft SQL schema parameter
    /// </summary>
    public class MsSqlDbObjectParam : MsSqlDbObjectBase
    {
        /// <summary>
        /// Query for select params from db
        /// </summary>
        public static string SelectQuery { get; } = @$"
SELECT d.name as {nameof(SchemaName)},
        b.name as {nameof(ObjectName)},
        b.[type] AS {nameof(ObjectType)},
        a.name as {nameof(ParamName)},
        c.name as {nameof(TypeName)},
        a.[prec] AS {nameof(TypeLength)},
        a.xprec AS {nameof(TypePrecision)},
        a.isnullable AS {nameof(IsNullable)},
        a.colid AS {nameof(OrderId)},
        a.[collation] AS [{nameof(Collation)}]
FROM syscolumns a WITH(NOLOCK)
JOIN sysobjects b WITH(NOLOCK) ON a.id = b.id
JOIN systypes c WITH(NOLOCK) ON a.xtype = c.xtype AND c.xtype=c.xusertype
JOIN sys.schemas d WITH(NOLOCK) ON b.[uid] = d.[schema_id] AND d.name != 'sys'
LEFT JOIN sys.tables t ON b.id = t.object_id
ORDER BY {nameof(SchemaName)}, {nameof(ObjectName)}, {nameof(ParamName)}
";
        /// <summary>
        /// Name of database parameter
        /// </summary>
        public string ParamName { get; protected set; }

        /// <summary>
        /// Name of database parameter type
        /// </summary>
        public string TypeName { get; protected set; }

        /// <summary>
        /// Database parameter type length
        /// </summary>
        public short TypeLength { get; protected set; }

        /// <summary>
        /// Database parameter type precision
        /// </summary>
        public byte TypePrecision { get; protected set; }

        /// <summary>
        /// Database parameter type precision
        /// </summary>
        public bool IsNullable { get; protected set; }

        /// <summary>
        /// Order id of this parameter
        /// </summary>
        public short OrderId { get; protected set; }

        /// <summary>
        /// Collation of this parameter
        /// </summary>
        public string? Collation { get; protected set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public MsSqlDbObjectParam()
        {
            ParamName  = IDeSerializable.DUMMY_STRING;
            TypeName   = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// Read data from db result
        /// </summary>
        /// <param name="reader">DB reader</param>
        public override void DbRead(IDbDataReader reader)
        {
            base.DbRead(reader);

            ParamName     = reader.ReadString(nameof(ParamName));
            TypeName      = reader.ReadString(nameof(TypeName));
            TypeLength    = reader.ReadInt16(nameof(TypeLength));
            TypePrecision = reader.ReadByte(nameof(TypePrecision));
            IsNullable    = SimpleConvert.Convert<bool>(reader.ReadInt32(nameof(IsNullable)));
            OrderId       = reader.ReadInt16(nameof(OrderId));
            Collation     = reader.ReadStringN(nameof(Collation));
        }
    }
}