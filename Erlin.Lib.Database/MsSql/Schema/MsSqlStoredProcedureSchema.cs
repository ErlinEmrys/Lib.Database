using System;
using System.Collections.Generic;
using System.Text;

namespace Erlin.Lib.Database.MsSql.Schema
{
    /// <summary>
    /// Microsoft SQL stored procedure schema
    /// </summary>
    public class MsSqlStoredProcedureSchema : MsSqlDbObjectBase
    {
        private readonly List<MsSqlDbObjectParam> _prms = new List<MsSqlDbObjectParam>();
        private readonly List<MsSqlDbObjectText> _texts = new List<MsSqlDbObjectText>();

        /// <summary>
        /// Ctor
        /// </summary>
        public MsSqlStoredProcedureSchema()
        {
            ObjectType = MsSqlDbObjectType.P;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="schemaName">SP schema name</param>
        /// <param name="spName">SP name</param>
        public MsSqlStoredProcedureSchema(string schemaName, string spName) : base(schemaName, spName, MsSqlDbObjectType.P)
        {
        }

        /// <summary>
        /// Add argument to this SP
        /// </summary>
        /// <param name="param">Column parameter</param>
        public void AddParam(MsSqlDbObjectParam param)
        {
            _prms.Add(param);
        }

        /// <summary>
        /// Add text definition to this SP
        /// </summary>
        /// <param name="textDefinition">Text definition</param>
        public void AddText(MsSqlDbObjectText textDefinition)
        {
            _texts.Add(textDefinition);
        }

        /// <summary>
        /// Generate creation script for this able
        /// </summary>
        /// <param name="result">Creation script</param>
        public void GenerateCreateScript(StringBuilder result)
        {
        }
    }
}