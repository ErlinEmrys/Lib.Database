using System;
using System.Collections.Generic;
using System.Text;

using Erlin.Lib.Common.Serialization;

namespace Erlin.Lib.Database.PgSql.Schema
{
    /// <summary>
    /// PostgreSql database schema
    /// </summary>
    public class PgSqlDbSchema
    {
        /// <summary>
        /// Name of server from which this schema originates
        /// </summary>
        public string ServerName { get; protected set; }

        /// <summary>
        /// Name of database from which this schema originates
        /// </summary>
        public string DatabaseName { get; protected set; }

        /// <summary>
        /// Ctor
        /// </summary>
        public PgSqlDbSchema()
        {
            ServerName = IDeSerializable.DUMMY_STRING;
            DatabaseName = IDeSerializable.DUMMY_STRING;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="serverName">Name of server from which this schema originates</param>
        /// <param name="databaseName">Name of database from which this schema originates</param>
        public PgSqlDbSchema(string serverName, string databaseName)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
        }

        /// <summary>
        /// Read schema from database
        /// </summary>
        /// <param name="connect">Open connection</param>
        public void ReadSchema(PgSqlDbConnect connect)
        {
            /*
            string sqlQuery = MsSqlDbObjectParam.SelectQuery + MsSqlDbObjectText.SelectQuery;
            MsSqlDataReader reader = connect.GetDataReader(sqlQuery, null);
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
            */
        }

        /// <summary>
        /// Generate creation script for all objects in scheme
        /// </summary>
        /// <returns>Creation script</returns>
        public StringBuilder GenerateCreateScript()
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("DUMMY PgSql script");

            /*
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
            */

            return result;
        }
    }
}
