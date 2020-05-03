using System;

using Erlin.Lib.Common;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Schema of one database checkedObjectTrigger
    /// </summary>
    [DebuggerDisplay("TRG: [{DbSchemaName}].[{Name}]")]
    public class DbObjectTriggerSchema : DbObjectSchemaBase
    {
        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return DbObjectType.Trigger; } }

        /// <summary>
        /// Databse object ID
        /// </summary>
        public long DbObjectId { get; protected set; }

        /// <summary>
        /// Databse schema name
        /// </summary>
        public string DbSchemaName { get; protected set; }

        /// <summary>
        /// SQL script of this object
        /// </summary>
        public string ScriptText { get; set; } = string.Empty;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbObjectId">Databse object ID</param>
        /// <param name="dbSchemaName">Databse schema name</param>
        public DbObjectTriggerSchema(string name, long dbObjectId, string dbSchemaName) : base(name)
        {
            DbObjectId = dbObjectId;
            DbSchemaName = dbSchemaName;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectTriggerSchema()
        {
            DbSchemaName = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public override void DeSerialize(IObjectReadWriter rw)
        {
            byte _ = rw.ReadWriteVersion(0);
            Name = rw.ReadWriteString(nameof(Name), Name);

            DbObjectId = rw.ReadWriteInt64(nameof(DbObjectId), DbObjectId);
            DbSchemaName = rw.ReadWriteString(nameof(DbSchemaName), DbSchemaName);
            ScriptText = rw.ReadWriteString(nameof(ScriptText), ScriptText);
        }

        /// <summary>
        /// Check if another Db object have same DB name
        /// </summary>
        /// <param name="trigger">Another DB object</param>
        /// <returns>True - objects have same names</returns>
        public bool IsSameDbName(DbObjectTriggerSchema trigger)
        {
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            return IsSameDbName(trigger.DbSchemaName, trigger.Name);
        }

        /// <summary>
        /// Check if entered DB name equals objects DB name
        /// </summary>
        /// <param name="dbSchemaName">DB schema name</param>
        /// <param name="dbName">DB object name</param>
        /// <returns>True - this object has same DB name</returns>
        public bool IsSameDbName(string dbSchemaName, string dbName)
        {
            return DbSchemaName.CompareIgnoreCase(dbSchemaName) && Name.CompareIgnoreCase(dbName);
        }

        /// <summary>
        /// Returns full database name/identifier
        /// </summary>
        /// <returns>Full name of the database object</returns>
        public override string GetFullName()
        {
            return $"[{DbSchemaName}].[{Name}]";
        }

        /// <summary>
        /// Compares database triggers if schematicly match each other
        /// </summary>
        /// <param name="masterObjectTrigger">Master database trigger (or left)</param>
        /// <param name="checkedObjectTrigger">Checked database trigger (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectTriggerSchema masterObjectTrigger, DbObjectTriggerSchema checkedObjectTrigger)
        {
            if (checkedObjectTrigger == null)
            {
                throw new ArgumentNullException(nameof(checkedObjectTrigger));
            }

            DbSchemaCompareResult result = new DbSchemaCompareResult(masterObjectTrigger, checkedObjectTrigger);
            if (string.Equals(masterObjectTrigger.ScriptText, checkedObjectTrigger.ScriptText, StringComparison.Ordinal))
            {
                result.CompareResultType = DbSchemaCompareResultType.Equals;
            }
            else
            {
                result.CompareResultType = DbSchemaCompareResultType.Different;
            }

            return result;
        }
    }
}