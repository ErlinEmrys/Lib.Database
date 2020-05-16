using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Erlin.Lib.Database.MsSql.Schema
{
    /// <summary>
    /// Microsoft SQL table schema
    /// </summary>
    public class MsSqlTableSchema : MsSqlDbObjectBase
    {
        private readonly List<MsSqlDbObjectParam> _prms = new List<MsSqlDbObjectParam>();

        /// <summary>
        /// Ctor
        /// </summary>
        public MsSqlTableSchema()
        {
            ObjectType = MsSqlDbObjectType.U;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="schemaName">Table schema name</param>
        /// <param name="tableName">Table name</param>
        public MsSqlTableSchema(string schemaName, string tableName) : base(schemaName, tableName, MsSqlDbObjectType.U)
        {
        }

        /// <summary>
        /// Add column paameter to this table
        /// </summary>
        /// <param name="param">Column parameter</param>
        public void AddParam(MsSqlDbObjectParam param)
        {
            _prms.Add(param);
        }

        /// <summary>
        /// Generate creation script for this able
        /// </summary>
        /// <param name="result">Creation script</param>
        public void GenerateCreateScript(StringBuilder result)
        {
            _prms.Sort((l, r) => l.OrderId.CompareTo(r.OrderId));

            result.AppendLine($"CREATE TABLE {ObjectIdentifier}(");
            foreach (MsSqlDbObjectParam fParam in _prms)
            {
                string typeLength = string.Empty;
                if (fParam.ParamType == SqlDbType.NVarChar)
                {
                    typeLength = $"({fParam.TypeLength})";
                }

                string notNull = string.Empty;
                if (!fParam.IsNullable)
                {
                    notNull += "NOT ";
                }

                result.AppendLine($"    [{fParam.ParamName}] [{fParam.ParamType}]{typeLength} {notNull}NULL,");
            }
            result.AppendLine($") ON [PRIMARY]");
            result.AppendLine($"GO");
            result.AppendLine($"");
        }
    }
}