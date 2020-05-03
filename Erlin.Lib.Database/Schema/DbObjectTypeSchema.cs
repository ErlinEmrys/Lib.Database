using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Schema of one database data checkedObjectType
    /// </summary>
    [DebuggerDisplay("{SqlType}[{Lenght}] ({Precision}/{Scale}) Null:{AllowNull}")]
    public class DbObjectTypeSchema : DbObjectSchemaBase
    {
        /// <summary>
        /// Type of database object
        /// </summary>
        public override DbObjectType ObjectType { get { return DbObjectType.DatabaseType; } }

        /// <summary>
        /// Name of the object
        /// </summary>
        public new string Name
        {
            get
            {
                return SqlType.ToString();
            }
            set
            {
                throw new InvalidOperationException("Cannot set DbObjectTypeSchema.Name! Name is derived from checkedObjectType.");
            }
        }

        /// <summary>
        /// Sql checkedObjectType
        /// </summary>
        public SqlDbType SqlType { get; set; }

        /// <summary>
        /// Lenght of the checkedObjectType
        /// </summary>
        public int Lenght { get; set; }

        /// <summary>
        /// Precision of the checkedObjectType
        /// </summary>
        public int Precision { get; set; }

        /// <summary>
        /// Scale of the checkedObjectType
        /// </summary>
        public int Scale { get; set; }

        /// <summary>
        /// Is checkedObjectType nullable
        /// </summary>
        public bool AllowNull { get; set; }

        /// <summary>
        /// (De)Serialize this object
        /// </summary>
        /// <param name="rw">Reader/writer</param>
        public override void DeSerialize(IObjectReadWriter rw)
        {
            byte _ = rw.ReadWriteVersion(0);
            SqlType = rw.ReadWriteEnum(nameof(SqlType), SqlType);

            Lenght = rw.ReadWriteInt32(nameof(Lenght), Lenght);
            Precision = rw.ReadWriteInt32(nameof(Precision), Precision);
            Scale = rw.ReadWriteInt32(nameof(Scale), Scale);
            AllowNull = rw.ReadWriteBool(nameof(AllowNull), AllowNull);
        }

        /// <summary>
        /// Returns full database name/identifier
        /// </summary>
        /// <returns>Full name of the database object</returns>
        public override string GetFullName()
        {
            return Name;
        }

        /// <summary>
        /// Compares database data-types if schematicly match each other
        /// </summary>
        /// <param name="masterObjectType">Master database data type (or left)</param>
        /// <param name="checkedObjectType">Checked database data type (or right)</param>
        /// <returns>Comparison result</returns>
        public static DbSchemaCompareResult DbCompare(DbObjectTypeSchema masterObjectType, DbObjectTypeSchema checkedObjectType)
        {
            if (masterObjectType == null)
            {
                throw new ArgumentNullException(nameof(masterObjectType));
            }

            if (checkedObjectType == null)
            {
                throw new ArgumentNullException(nameof(checkedObjectType));
            }

            bool equal = masterObjectType.SqlType == checkedObjectType.SqlType && masterObjectType.Lenght == checkedObjectType.Lenght &&
                         masterObjectType.Precision == checkedObjectType.Precision && masterObjectType.Scale == checkedObjectType.Scale && masterObjectType.AllowNull == checkedObjectType.AllowNull;

            DbSchemaCompareResult result = new DbSchemaCompareResult(masterObjectType, checkedObjectType);
            if (equal)
            {
                result.CompareResultType = DbSchemaCompareResultType.Equals;
            }
            else
            {
                result.CompareResultType = DbSchemaCompareResultType.Different;
            }

            return result;
        }

        internal string GetScript()
        {
            string length = Lenght > 0 ? $"({Lenght})" : string.Empty;
            return $"[{Name}]{length}";
        }
    }
}