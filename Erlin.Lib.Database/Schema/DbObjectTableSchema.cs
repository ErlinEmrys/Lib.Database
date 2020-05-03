using System;

using Erlin.Lib.Common;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Schema of one database checkedObjectTable
    /// </summary>
    [DebuggerDisplay("T: [{DbSchemaName}].[{Name}]")]
    public class DbObjectTableSchema : DbObjectSchemaBase
    {
        private List<DbObjectTableColumnSchema> _columns = new List<DbObjectTableColumnSchema>();

        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return DbObjectType.Table; } }

        /// <summary>
        /// Database schema name
        /// </summary>
        public string DbSchemaName { get; set; }

        /// <summary>
        /// Table columns
        /// </summary>
        public IReadOnlyCollection<DbObjectTableColumnSchema> Columns { get { return new ReadOnlyCollection<DbObjectTableColumnSchema>(_columns); } }

        /// <summary>
        /// Table triggers
        /// </summary>
        public List<DbObjectTriggerSchema> Triggers { get; private set; } = new List<DbObjectTriggerSchema>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbSchemaName">Database schema name</param>
        public DbObjectTableSchema(string name, string dbSchemaName) : base(name)
        {
            DbSchemaName = dbSchemaName;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectTableSchema()
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

            DbSchemaName = rw.ReadWriteString(nameof(DbSchemaName), DbSchemaName);
            _columns = rw.ReadWriteList(nameof(_columns), _columns);
            Triggers = rw.ReadWriteList(nameof(Triggers), Triggers);
        }

        /// <summary>
        /// Add new DB column schema to this DB table schema
        /// </summary>
        /// <param name="column">Db table column schema to add</param>
        public void AddColumn(DbObjectTableColumnSchema column)
        {
            _columns.Add(column);

            int fakeOrderId = 0;
            _columns = _columns.OrderBy(c => c.OrderId).ToList();
            _columns.ForEach(c => c.FakeOrderId = fakeOrderId++);
        }

        /// <summary>
        /// Check if another Db object have same DB name
        /// </summary>
        /// <param name="trigger">Another DB object</param>
        /// <returns>True - objects have same names</returns>
        public bool IsSameDbName(DbObjectTableSchema trigger)
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
        /// Compares database tables if schematicly match each other
        /// </summary>
        /// <param name="masterObjectTable">Master database objectTable (or left)</param>
        /// <param name="checkedObjectTable">Checked database objectTable (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectTableSchema masterObjectTable, DbObjectTableSchema checkedObjectTable)
        {
            if (checkedObjectTable == null)
            {
                throw new ArgumentNullException(nameof(checkedObjectTable));
            }

            DbSchemaCompareResult result = new DbSchemaCompareResult(masterObjectTable, checkedObjectTable);
            result.CompareResultType = DbSchemaCompareResultType.Equals;

            CompareCollection(result, masterObjectTable.Columns, checkedObjectTable.Columns, (left, right) => string.Equals(left.Name, right.Name, StringComparison.Ordinal),
                              DbObjectTableColumnSchema.DbCompare);

            foreach (DbSchemaCompareResult fColumnCompare in result.InnerResults)
            {
                if (fColumnCompare.CompareResultType != DbSchemaCompareResultType.Equals)
                {
                    fColumnCompare.CompareResultType = DbSchemaCompareResultType.Different;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns create script of this table
        /// </summary>
        /// <returns></returns>
        public string GetCreateScript()
        {
            IOrderedEnumerable<DbObjectTableColumnSchema> col = Columns.OrderBy(c => c.FakeOrderId);
            string columns = string.Join(",\r\n", col.Select(c => c.GetScript()));

            string sql = $@"CREATE TABLE {GetFullName()}({columns})";

            return sql;
        }
    }
}