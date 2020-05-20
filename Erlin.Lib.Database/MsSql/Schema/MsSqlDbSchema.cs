using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Erlin.Lib.Common.Exceptions;
using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.MsSql.Schema
{
    /// <summary>
    /// Microsoft SQL database schema
    /// </summary>
    public class MsSqlDbSchema
    {
        private readonly List<MsSqlStoredProcedureSchema> _storedProcedures = new List<MsSqlStoredProcedureSchema>();
        private readonly List<MsSqlTableSchema> _tables = new List<MsSqlTableSchema>();

        /// <summary>
        /// Name of server from which this schema originates
        /// </summary>
        public string ServerName { get; protected set; }

        /// <summary>
        /// Name of database from which this schema originates
        /// </summary>
        public string DatabaseName { get; protected set; }

        /// <summary>
        /// Tables
        /// </summary>
        public ReadOnlyCollection<MsSqlTableSchema> Tables { get { return _tables.AsReadOnly(); } }

        /// <summary>
        /// Stored procedures
        /// </summary>
        public ReadOnlyCollection<MsSqlStoredProcedureSchema> StoredProcedures { get { return _storedProcedures.AsReadOnly(); } }

        /// <summary>
        /// Ctor
        /// </summary>
        public MsSqlDbSchema()
        {
            ServerName = IDeSerializable.DUMMY_STRING;
            DatabaseName = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serverName">Name of server from which this schema originates</param>
        /// <param name="databaseName">Name of database from which this schema originates</param>
        public MsSqlDbSchema(string serverName, string databaseName)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
        }

        /// <summary>
        /// Read schema from database
        /// </summary>
        /// <param name="connect">Open connection</param>
        public void ReadSchema(MsSqlDbConnect connect)
        {
            string sqlQuery = MsSqlDbObjectParam.SelectQuery + MsSqlDbObjectText.SelectQuery;
            MsSqlDataReader reader = connect.GetDataReader(sqlQuery);
            List<MsSqlDbObjectParam> prms = reader.ReadList<MsSqlDbObjectParam>();
            if (!reader.NextResult())
            {
                throw new InvalidOperationException("Expected at least two result sets!");
            }

            List<MsSqlDbObjectText> texts = reader.ReadList<MsSqlDbObjectText>();

            foreach (MsSqlDbObjectParam fParam in prms)
            {
                switch (fParam.ObjectType)
                {
                    case MsSqlDbObjectType.U:
                        AddToTable(fParam);
                        break;
                    case MsSqlDbObjectType.P:
                        AddToStoredProcedure(fParam);
                        break;
                    default:
                        throw new EnumValueNotImplementedException(fParam.ObjectType);
                }
            }

            foreach (MsSqlDbObjectText fText in texts)
            {
                switch (fText.ObjectType)
                {
                    case MsSqlDbObjectType.P:
                        AddToStoredProcedure(fText);
                        break;
                    default:
                        throw new EnumValueNotImplementedException(fText.ObjectType);
                }
            }
        }

        /// <summary>
        /// Generate creation script for all objects in scheme
        /// </summary>
        /// <returns>Creation script</returns>
        public StringBuilder GenerateCreateScript()
        {
            StringBuilder result = new StringBuilder();

            _tables.Sort((l, r) => String.Compare(l.ObjectIdentifier, r.ObjectIdentifier, StringComparison.InvariantCulture));
            _storedProcedures.Sort((l, r) => String.Compare(l.ObjectIdentifier, r.ObjectIdentifier, StringComparison.InvariantCulture));

            result.AppendLine("--TABLES");
            foreach (MsSqlTableSchema fTable in _tables)
            {
                fTable.GenerateCreateScript(result);
            }

            result.AppendLine("--STORED PROCEDURES");
            foreach (MsSqlStoredProcedureSchema fProcedure in _storedProcedures)
            {
                fProcedure.GenerateCreateScript(result);
            }

            return result;
        }

        /// <summary>
        /// Add parameter to table
        /// </summary>
        /// <param name="prms">Parameter to add</param>
        private void AddToTable(MsSqlDbObjectParam prms)
        {
            MsSqlTableSchema table = _tables.SingleOrDefault(t => t.SchemaName == prms.SchemaName && t.ObjectName == prms.ObjectName);
            if (table == null)
            {
                table = new MsSqlTableSchema(prms.SchemaName, prms.ObjectName);
                _tables.Add(table);
            }

            table.AddParam(prms);
        }

        /// <summary>
        /// Add parameter to stored procedure
        /// </summary>
        /// <param name="prms">Parameter to add</param>
        private void AddToStoredProcedure(MsSqlDbObjectParam prms)
        {
            MsSqlStoredProcedureSchema sp = _storedProcedures.SingleOrDefault(p => p.SchemaName == prms.SchemaName && p.ObjectName == prms.ObjectName);
            if (sp == null)
            {
                sp = new MsSqlStoredProcedureSchema(prms.SchemaName, prms.ObjectName);
                _storedProcedures.Add(sp);
            }

            sp.AddParam(prms);
        }

        /// <summary>
        /// Add text to stored procedure
        /// </summary>
        /// <param name="text">Text to add</param>
        private void AddToStoredProcedure(MsSqlDbObjectText text)
        {
            MsSqlStoredProcedureSchema sp = _storedProcedures.SingleOrDefault(p => p.SchemaName == text.SchemaName && p.ObjectName == text.ObjectName);
            if (sp == null)
            {
                sp = new MsSqlStoredProcedureSchema(text.SchemaName, text.ObjectName);
                _storedProcedures.Add(sp);
            }

            sp.AddText(text);
        }
    }
}