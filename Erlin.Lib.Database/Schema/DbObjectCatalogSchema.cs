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
    /// Schema of database (checkedObjectCatalog)
    /// </summary>
    [DebuggerDisplay("DB: {Name} [{Server}]")]
    public class DbObjectCatalogSchema : DbObjectSchemaBase
    {
        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return DbObjectType.DatabaseCatalog; } }

        /// <summary>
        /// Server hosting this database
        /// </summary>
        public string Server { get; protected set; }

        /// <summary>
        /// Stored procedures
        /// </summary>
        public List<DbObjectStoredProcedureSchema> StoredProcedures { get; private set; } = new List<DbObjectStoredProcedureSchema>();

        /// <summary>
        /// Functions
        /// </summary>
        public List<DbObjectFunctionSchema> Functions { get; private set; } = new List<DbObjectFunctionSchema>();

        /// <summary>
        /// Table valued functions (functions that returns tables)
        /// </summary>
        public List<DbObjectTableValuedFunctionSchema> TableValuedFunctions { get; private set; } = new List<DbObjectTableValuedFunctionSchema>();

        /// <summary>
        /// Tables
        /// </summary>
        public List<DbObjectTableSchema> Tables { get; private set; } = new List<DbObjectTableSchema>();

        /// <summary>
        /// Views
        /// </summary>
        public List<DbObjectViewSchema> Views { get; } = new List<DbObjectViewSchema>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="server">Server hosting this database</param>
        public DbObjectCatalogSchema(string name, string server) : base(name)
        {
            Server = server;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        public DbObjectCatalogSchema()
        {
            Server = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public override void DeSerialize(IObjectReadWriter rw)
        {
            byte _ = rw.ReadWriteVersion(0);
            Name = rw.ReadWriteString(nameof(Name), Name);

            Server = rw.ReadWriteString(nameof(Server), Server);
            StoredProcedures = rw.ReadWriteList(nameof(StoredProcedures), StoredProcedures);
            Functions = rw.ReadWriteList(nameof(Functions), Functions);
            TableValuedFunctions = rw.ReadWriteList(nameof(TableValuedFunctions), TableValuedFunctions);
            Tables = rw.ReadWriteList(nameof(Tables), Tables);
        }

        /// <summary>
        /// Returns full database name/identifier
        /// </summary>
        /// <returns></returns>
        public override string GetFullName()
        {
            return $"[{Server}] {Name}";
        }

        /// <summary>
        /// Compares databases if schematicly match each other
        /// </summary>
        /// <param name="masterObjectCatalog">Master database catalog (or left)</param>
        /// <param name="checkedObjectCatalog">Checked database catalog (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectCatalogSchema masterObjectCatalog, DbObjectCatalogSchema checkedObjectCatalog)
        {
            if (checkedObjectCatalog == null)
            {
                throw new ArgumentNullException(nameof(checkedObjectCatalog));
            }

            DbSchemaCompareResult result = new DbSchemaCompareResult(masterObjectCatalog, checkedObjectCatalog);

            //SP
            CompareCollection(result, masterObjectCatalog.StoredProcedures, checkedObjectCatalog.StoredProcedures,
                              (left, right) => left.IsSameDbName(right), DbObjectStoredProcedureSchema.DbCompare);

            //FN
            CompareCollection(result, masterObjectCatalog.Functions, checkedObjectCatalog.Functions,
                              (left, right) => left.IsSameDbName(right), DbObjectFunctionSchema.DbCompare);

            //TFN
            CompareCollection(result, masterObjectCatalog.TableValuedFunctions, checkedObjectCatalog.TableValuedFunctions,
                              (left, right) => left.IsSameDbName(right), DbObjectFunctionSchema.DbCompare);

            //Tables
            CompareCollection(result, masterObjectCatalog.Tables, checkedObjectCatalog.Tables,
                              (left, right) => left.IsSameDbName(right), DbObjectTableSchema.DbCompare);

            //Views
            CompareCollection(result, masterObjectCatalog.Views, checkedObjectCatalog.Views,
                              (left, right) => left.IsSameDbName(right), DbObjectViewSchema.DbCompare);


            return result;
        }
    }
}