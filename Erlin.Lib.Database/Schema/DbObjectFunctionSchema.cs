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
    /// Schema of one database checkedObjectFunction
    /// </summary>
    [DebuggerDisplay("FN: [{DbSchemaName}].[{Name}]")]
    public class DbObjectFunctionSchema : DbObjectStoredProcedureSchema
    {
        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return DbObjectType.Function; } }

        /// <summary>
        /// Return parameter
        /// </summary>
        public List<DbObjectParameterSchema> ReturnValues { get; set; } = new List<DbObjectParameterSchema>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbObjectId">Databse object ID</param>
        /// <param name="dbSchemaName">Databse schema name</param>
        public DbObjectFunctionSchema(string name, long dbObjectId, string dbSchemaName) : base(name, dbObjectId, dbSchemaName)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectFunctionSchema()
        {
        }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public override void DeSerialize(IObjectReadWriter rw)
        {
            byte _ = rw.ReadWriteVersion(0);
            base.DeSerialize(rw);

            ReturnValues = rw.ReadWriteList(nameof(ReturnValues), ReturnValues, c =>
                                                                                {
                                                                                    if (c.TypeName.EndsWith(typeof(DbObjectTableColumnSchema).Name, StringComparison.InvariantCulture))
                                                                                    {
                                                                                        c.Instance = new DbObjectTableColumnSchema();
                                                                                    }
                                                                                    else if (c.TypeName.EndsWith(typeof(DbObjectParameterSchema).Name, StringComparison.InvariantCulture))
                                                                                    {
                                                                                        c.Instance = new DbObjectParameterSchema();
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        throw new NotImplementedException();
                                                                                    }
                                                                                });
        }

        /// <summary>
        /// Compares databas functions if schematicly match each other
        /// </summary>
        /// <param name="masterObjectFunction">Master database function (or left)</param>
        /// <param name="checkedObjectFunction">Checked database function (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectFunctionSchema masterObjectFunction, DbObjectFunctionSchema checkedObjectFunction)
        {
            if (masterObjectFunction == null)
            {
                throw new ArgumentNullException(nameof(masterObjectFunction));
            }

            if (checkedObjectFunction == null)
            {
                throw new ArgumentNullException(nameof(checkedObjectFunction));
            }

            //masterObjectFunction.ReturnValues.Sort((l, r) => l.FakeOrderId.CompareTo(r.FakeOrderId));
            //checkedObjectFunction.ReturnValues.Sort((l, r) => l.FakeOrderId.CompareTo(r.FakeOrderId));

            DbSchemaCompareResult result = DbObjectStoredProcedureSchema.DbCompare(masterObjectFunction, checkedObjectFunction);

            foreach (DbObjectParameterSchema fMasterParam in masterObjectFunction.ReturnValues)
            {
                DbObjectParameterSchema fCheckParam = checkedObjectFunction.ReturnValues.FirstOrDefault(p => p.Name == fMasterParam.Name);
                result.InnerResults.Add(DbObjectParameterSchema.DbCompare(fMasterParam, fCheckParam));
            }

            foreach (DbObjectParameterSchema fCheckParam in checkedObjectFunction.ReturnValues)
            {
                DbObjectParameterSchema? fMasterParam = masterObjectFunction.ReturnValues.FirstOrDefault(p => p.Name == fCheckParam.Name);
                if (fMasterParam == null)
                {
                    result.InnerResults.Add(DbObjectParameterSchema.DbCompare(null, fCheckParam));
                }
            }

            return result;
        }
    }
}