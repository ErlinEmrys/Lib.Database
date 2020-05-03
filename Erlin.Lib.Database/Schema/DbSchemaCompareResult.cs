using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Erlin.Lib.Database.Schema
{
    /// <summary>
    /// Comparison result of database schemas
    /// </summary>
    [DebuggerDisplay("{ObjectFullName} [{CompareResultType}]")]
    public class DbSchemaCompareResult
    {
        private DbSchemaCompareResultType _compareResultType;

        /// <summary>
        /// Type of compared database object
        /// </summary>
        public DbObjectType DbObjectType
        {
            get
            {
                return MasterBase?.ObjectType ?? CheckedBase?.ObjectType ?? throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Resolution of comparison
        /// </summary>
        public DbSchemaCompareResultType CompareResultType
        {
            get
            {
                DbSchemaCompareResultType result = _compareResultType;
                foreach (DbSchemaCompareResult fResult in InnerResults)
                {
                    result |= fResult.CompareResultType;
                }

                return result;
            }
            set
            {
                _compareResultType = value;
            }
        }

        /// <summary>
        /// Master object
        /// </summary>
        public DbObjectSchemaBase? MasterBase { get; }

        /// <summary>
        /// Checked object
        /// </summary>
        public DbObjectSchemaBase? CheckedBase { get; }

        /// <summary>
        /// Simple object name
        /// </summary>
        public string ObjectFullName
        {
            get
            {
                if (MasterBase != null)
                {
                    return MasterBase.GetFullName();
                }

                if (CheckedBase != null)
                {
                    return CheckedBase.GetFullName();
                }

                return "Unknown";
            }
        }

        /// <summary>
        /// Results of inner comparisons
        /// </summary>
        public List<DbSchemaCompareResult> InnerResults { get; } = new List<DbSchemaCompareResult>();

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="masterBase">Master database object</param>
        /// <param name="checkedBase">Checked database object</param>
        public DbSchemaCompareResult(DbObjectSchemaBase? masterBase, DbObjectSchemaBase? checkedBase)
        {
            MasterBase = masterBase;
            CheckedBase = checkedBase;
        }
    }
}