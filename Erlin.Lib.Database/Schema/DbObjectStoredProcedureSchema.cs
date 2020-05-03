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
    /// Schema of database stored procedure
    /// </summary>
    [DebuggerDisplay("SP: [{DbSchemaName}].[{Name}]")]
    public class DbObjectStoredProcedureSchema : DbObjectTriggerSchema
    {
        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return DbObjectType.StoredProcedure; } }

        /// <summary>
        /// Parameters
        /// </summary>
        public List<DbObjectParameterSchema> Parameters { get; private set; } = new List<DbObjectParameterSchema>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="dbObjectId">Databse object ID</param>
        /// <param name="dbSchemaName">Databse schema name</param>
        public DbObjectStoredProcedureSchema(string name, long dbObjectId, string dbSchemaName) : base(name, dbObjectId, dbSchemaName)
        {
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectStoredProcedureSchema()
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

            Parameters = rw.ReadWriteList(nameof(Parameters), Parameters, c =>
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
        /// Compares database stored procedures if schematicly match each other
        /// </summary>
        /// <param name="masterSp">Master database stored procedure (or left)</param>
        /// <param name="checkedSp">Checked database stored procedure (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectStoredProcedureSchema masterSp, DbObjectStoredProcedureSchema checkedSp)
        {
            if (masterSp == null)
            {
                throw new ArgumentNullException(nameof(masterSp));
            }

            if (checkedSp == null)
            {
                throw new ArgumentNullException(nameof(checkedSp));
            }

            DbSchemaCompareResult result = DbObjectTriggerSchema.DbCompare(masterSp, checkedSp);

            CompareCollection(result, masterSp.Parameters, checkedSp.Parameters, (left, right) => string.Equals(left.Name, right.Name, StringComparison.Ordinal),
                              DbObjectParameterSchema.DbCompare);

            return result;
        }
    }
}