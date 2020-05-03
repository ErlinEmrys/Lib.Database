using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Schema of one database table column
    /// </summary>
    [DebuggerDisplay("{Name} [{DbObjectType.SqlType}[{DbObjectType.Lenght}] ({DbObjectType.Precision}/{DbObjectType.Scale}) Null:{DbObjectType.AllowNull}] ({DefaultValue}) [{FakeOrderId}({OrderId})]}")]
    public class DbObjectTableColumnSchema : DbObjectParameterSchema
    {
        /// <summary>
        /// Default value of column
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Column collation
        /// </summary>
        public string Collation { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbObjectType">Database type of this parameter</param>
        /// <param name="collation">Column collation</param>
        public DbObjectTableColumnSchema(string name, DbObjectTypeSchema dbObjectType, string collation) : base(name, dbObjectType)
        {
            Collation = collation;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectTableColumnSchema()
        {
            Collation = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public override void DeSerialize(IObjectReadWriter rw)
        {
            byte _ = rw.ReadWriteVersion(0);
            base.DeSerialize(rw);

            DefaultValue = rw.ReadWriteStringN(nameof(DefaultValue), DefaultValue);
            Collation = rw.ReadWriteString(nameof(Collation), Collation);
        }

        /// <summary>
        /// Compares database table columns if schematicly match each other
        /// </summary>
        /// <param name="masterColumn">Master database table column (or left)</param>
        /// <param name="checkedColumn">Checked database table column (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectTableColumnSchema masterColumn, DbObjectTableColumnSchema checkedColumn)
        {
            if (masterColumn == null)
            {
                throw new ArgumentNullException(nameof(masterColumn));
            }

            if (checkedColumn == null)
            {
                throw new ArgumentNullException(nameof(checkedColumn));
            }

            DbSchemaCompareResult result = DbObjectParameterSchema.DbCompare(masterColumn, checkedColumn);
            if (masterColumn.DefaultValue != checkedColumn.DefaultValue)
            {
                result.CompareResultType = DbSchemaCompareResultType.Different;
            }

            if (masterColumn.Collation != checkedColumn.Collation)
            {
                result.CompareResultType = DbSchemaCompareResultType.Different;
            }

            return result;
        }

        internal string GetScript()
        {
            string collate = string.Empty;
            if (!string.IsNullOrEmpty(Collation))
            {
                collate = $"COLLATE {Collation}";
            }

            string allowNull = DbObjectType.AllowNull ? "NULL" : "NOT NULL";
            return $"[{Name}] {DbObjectType.GetScript()} {collate} {allowNull}";
        }
    }
}