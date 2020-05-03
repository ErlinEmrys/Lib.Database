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
    /// Schema of database parameter (for stored procedures, functions, etc.)
    /// </summary>
    [DebuggerDisplay("{Name} [{DbObjectType}] {FakeOrderId}({OrderId})}")]
    public class DbObjectParameterSchema : DbObjectSchemaBase
    {
        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return Database.DbObjectType.Parameter; } }

        /// <summary>
        /// Database type of this parameter
        /// </summary>
        public DbObjectTypeSchema DbObjectType { get; protected set; }

        /// <summary>
        /// Order of this paramaeter
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Fake (real) order of this paramaeter
        /// </summary>
        public int FakeOrderId { get; set; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbObjectType">Database type of this parameter</param>
        public DbObjectParameterSchema(string name, DbObjectTypeSchema dbObjectType) : base(name)
        {
            DbObjectType = dbObjectType;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectParameterSchema()
        {
            DbObjectType = new DbObjectTypeSchema();
        }

        /// <summary>
        /// Returns full database name/identifier
        /// </summary>
        /// <returns></returns>
        public override string GetFullName()
        {
            return Name;
        }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public override void DeSerialize(IObjectReadWriter rw)
        {
            byte _ = rw.ReadWriteVersion(0);
            Name = rw.ReadWriteString(nameof(Name), Name);

            DbObjectType = rw.ReadWrite(nameof(DbObjectType), DbObjectType);
            OrderId = rw.ReadWriteInt32(nameof(OrderId), OrderId);
            FakeOrderId = rw.ReadWriteInt32(nameof(FakeOrderId), FakeOrderId);
        }

        /// <summary>
        /// Compares database parameters if schematicly match each other
        /// </summary>
        /// <param name="masterParam">Master database catalog (or left)</param>
        /// <param name="checkedParam">Checked database catalog (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectParameterSchema? masterParam, DbObjectParameterSchema? checkedParam)
        {
            DbSchemaCompareResult result = new DbSchemaCompareResult(masterParam, checkedParam);
            if (checkedParam == null || masterParam == null)
            {
                result.CompareResultType = DbSchemaCompareResultType.Different;
            }
            else
            {
                result.InnerResults.Add(DbObjectTypeSchema.DbCompare(masterParam.DbObjectType, checkedParam.DbObjectType));
                if (masterParam.FakeOrderId == checkedParam.FakeOrderId)
                {
                    result.CompareResultType = DbSchemaCompareResultType.Equals;
                }
                else
                {
                    result.CompareResultType = DbSchemaCompareResultType.Different;
                }
            }

            return result;
        }
    }
}