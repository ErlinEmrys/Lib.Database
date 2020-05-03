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
    /// Schema of one database checkedObjectView
    /// </summary>
    [DebuggerDisplay("V: [{DbSchemaName}].[{Name}]")]
    public class DbObjectViewSchema : DbObjectSchemaBase
    {
        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return DbObjectType.View; } }

        /// <summary>
        /// Database schmea name
        /// </summary>
        public string DbSchemaName { get; set; }

        /// <summary>
        /// Text
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbSchemaName">Database schema name</param>
        /// <param name="text">Text</param>
        public DbObjectViewSchema(string name, string dbSchemaName, string text) : base(name)
        {
            DbSchemaName = dbSchemaName;
            Text = text;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectViewSchema()
        {
            DbSchemaName = IDeSerializable.DUMMY_STRING;
            Text = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public override void DeSerialize(IObjectReadWriter rw)
        {
            byte _ = rw.ReadWriteVersion(0);
            Name = rw.ReadWriteString(nameof(Name), Name);

            DbSchemaName = rw.ReadWriteString(nameof(DbSchemaName), DbSchemaName);
            Text = rw.ReadWriteString(nameof(Text), Text);
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
        /// Check if another Db object have same DB name
        /// </summary>
        /// <param name="trigger">Another DB object</param>
        /// <returns>True - objects have same names</returns>
        public bool IsSameDbName(DbObjectViewSchema trigger)
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
        /// Compares database views if schematicly match each other
        /// </summary>
        /// <param name="masterObjectView">Master database objectView (or left)</param>
        /// <param name="checkedObjectView">Checked database objectView (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectViewSchema masterObjectView, DbObjectViewSchema checkedObjectView)
        {
            if (checkedObjectView == null)
            {
                throw new ArgumentNullException(nameof(checkedObjectView));
            }

            DbSchemaCompareResult result = new DbSchemaCompareResult(masterObjectView, checkedObjectView);
            result.CompareResultType = DbSchemaCompareResultType.Equals;

            foreach (DbSchemaCompareResult fColumnCompare in result.InnerResults)
            {
                if (fColumnCompare.CompareResultType != DbSchemaCompareResultType.Equals)
                {
                    fColumnCompare.CompareResultType = DbSchemaCompareResultType.Different;
                }
            }

            return result;
        }
    }
}