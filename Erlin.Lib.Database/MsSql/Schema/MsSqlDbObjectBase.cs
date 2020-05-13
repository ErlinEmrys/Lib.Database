using System;
using System.Collections.Generic;
using System.Text;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.MsSql.Schema
{
    /// <summary>
    /// Microsoft SQL databse object parameter
    /// </summary>
    public abstract class MsSqlDbObjectBase : IDbReadable
    {
        /// <summary>
        /// Database schema name of database object
        /// </summary>
        public string SchemaName { get; protected set; }

        /// <summary>
        /// Name of database object parameter belongs to
        /// </summary>
        public string ObjectName { get; protected set; }

        /// <summary>
        /// Type of database object
        /// </summary>
        public MsSqlDbObjectType ObjectType { get; protected set; }

        /// <summary>
        /// Unique identifier of this object for database
        /// </summary>
        public string ObjectIdentifier { get { return $"[{SchemaName}].[{ObjectName}]"; } }

        /// <summary>
        /// Ctor
        /// </summary>
        protected MsSqlDbObjectBase()
        {
            SchemaName = IDeSerializable.DUMMY_STRING;
            ObjectName = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="schemaName">Database schema name of database object</param>
        /// <param name="objectName">Name of database object parameter belongs to</param>
        /// <param name="type">Type of database object</param>
        protected MsSqlDbObjectBase(string schemaName, string objectName, MsSqlDbObjectType type)
        {
            SchemaName = schemaName;
            ObjectName = objectName;
            ObjectType = type;
        }

        /// <summary>
        /// Read data from db result
        /// </summary>
        /// <param name="reader">DB reader</param>
        public virtual void DbRead(IDbDataReader reader)
        {
            SchemaName = reader.ReadString(nameof(SchemaName));
            ObjectName = reader.ReadString(nameof(ObjectName));
            ObjectType = SimpleConvert.Convert<MsSqlDbObjectType>(reader.ReadString(nameof(ObjectType)));
        }
    }
}