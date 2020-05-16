using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Exceptions;
using Erlin.Lib.Database.MsSql.Schema;
using Erlin.Lib.Database.Schema;

using Microsoft.Data.SqlClient;

namespace Erlin.Lib.Database.MsSql
{
    /// <summary>
    /// Microsoft SQL database connection object
    /// </summary>
    public sealed class MsSqlDbConnect : IDbConnect
    {
        /// <summary>
        /// Default timeout for every command (seconds)
        /// </summary>
        public const int DEFAULT_COMMAND_TIMEOUT_SECONDS = 500;
        private readonly SqlConnection _connection;
        private readonly string _connectionString;
        private SqlTransaction? _transaction;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connection">Existing connection object</param>
        public MsSqlDbConnect(SqlConnection connection)
        {
            _connection = connection;
            _connectionString = _connection.ConnectionString;
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString">MS-SQL connection string</param>
        public MsSqlDbConnect(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connection = new SqlConnection(connectionString);
            _connectionString = connectionString;
        }

        /// <summary>
        /// Open connection to database
        /// </summary>
        public void Open()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// Close connection to database
        /// </summary>
        public void Close()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// Read complete databae schema
        /// </summary>
        /// <returns>Parsed database schema</returns>
        public MsSqlDbSchema ReadSchema()
        {
            SqlConnectionStringBuilder connStringBuilder = new SqlConnectionStringBuilder(_connectionString);
            MsSqlDbSchema result = new MsSqlDbSchema(connStringBuilder.DataSource, connStringBuilder.InitialCatalog);
            result.ReadSchema(this);
            return result;
        }

        /// <summary>
        /// Returns dataset from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public DataSet GetDataSetQuery(string query, List<SqlParam> prms)
        {
            return GetDataSetImpl(query, CommandType.Text, prms);
        }

        //private static void GetReturnedValues(List<SqlParam> prms)
        //{
        //    if (prms != null)
        //    {
        //        foreach (SqlParam fParam in prms)
        //        {
        //            if (fParam.Direction != ParameterDirection.Input && fParam.SqlParameter != null)
        //            {
        //                fParam.Value = SimpleConvert.ConvertDbNullToNull(fParam.SqlParameter.Value);

        //                //Null original param object - we can re-use this object
        //                fParam.SqlParameter = null;
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _connection.Dispose();
        }

        /// <summary>
        /// Begin new database transaction
        /// </summary>
        /// <returns>Begined transaction</returns>
        public IDbTransaction BeginTransaction()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot begin another database transaction - transaction in progress!");
            }

            _transaction = _connection.BeginTransaction();
            return _transaction;
        }

        /// <summary>
        /// Commit current active database transaction
        /// </summary>
        public void CommitTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Cannot commit database transaction - transaction not exist!");
            }

            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        /// <summary>
        /// Make rollback of current active database transaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Cannot rollback database transaction - transaction not exist!");
            }

            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        private static SqlDbType ConvertToDbType(string input)
        {
            if (input == "numeric")
            {
                return SqlDbType.Decimal;
            }

            return SimpleConvert.Convert<SqlDbType>(input);
        }

        private static void ReadParamObjects(DataTable paramTable, Dictionary<long, string> defaultValues, DbObjectCatalogSchema result)
        {
            foreach (DataRow? fRow in paramTable.Rows)
            {
                if(fRow == null)
                {
                    continue;
                }

                bool isSystemObject = SimpleConvert.Convert<bool>(fRow["IsSystemObject"]);
                if (isSystemObject)
                {
                    continue;
                }

                string dbSchema = SimpleConvert.Convert<string>(fRow["SchemaName"]);
                string objectName = SimpleConvert.Convert<string>(fRow["ObjectName"]);
                string paramName = SimpleConvert.Convert<string>(fRow["ParamName"]);
                string typeName = SimpleConvert.Convert<string>(fRow["TypeName"]);
                int typeLength = SimpleConvert.Convert<int>(fRow["TypeLength"]);
                int typePrecision = SimpleConvert.Convert<int>(fRow["TypePrecision"]);
                int typeScale = SimpleConvert.Convert<int>(fRow["TypeScale"]);
                bool isNullable = SimpleConvert.Convert<bool>(fRow["IsNullable"]);
                MsSqlDbObjectType type = SimpleConvert.Convert<MsSqlDbObjectType>(fRow["ObjectType"]);
                int columnId = SimpleConvert.Convert<int>(fRow["ColumnId"]);
                long columndDefaultObjectId = SimpleConvert.Convert<long>(fRow["ColumndDefaultObjectId"]);
                string collation = SimpleConvert.Convert<string>(fRow["Collation"]);

                DbObjectTypeSchema dbObjectType = new DbObjectTypeSchema();
                dbObjectType.AllowNull = isNullable;
                dbObjectType.Lenght = typeLength;
                dbObjectType.Precision = typePrecision;
                dbObjectType.Scale = typeScale;
                dbObjectType.SqlType = ConvertToDbType(typeName);

                DbObjectTableColumnSchema column = new DbObjectTableColumnSchema(paramName, dbObjectType, collation);
                column.OrderId = columnId;
                if (defaultValues.ContainsKey(columndDefaultObjectId))
                {
                    column.DefaultValue = defaultValues[columndDefaultObjectId];
                }

                switch (type)
                {
                    case MsSqlDbObjectType.V:
                        break;
                    case MsSqlDbObjectType.FN:
                    case MsSqlDbObjectType.TF:
                        DbObjectFunctionSchema func = result.Functions.FirstOrDefault(f => f.IsSameDbName(dbSchema, objectName)) ??
                                                      result.TableValuedFunctions.FirstOrDefault(f => f.IsSameDbName(dbSchema, objectName));

                        if (func == null)
                        {
                            throw new InvalidOperationException("Function not found, but should be present!");
                        }
                        else if (string.IsNullOrEmpty(column.Name) || column.Name.StartsWith("@", StringComparison.Ordinal) || type == MsSqlDbObjectType.TF && column.Name == "value")
                        {
                            //Parameter without name == return value
                            func.ReturnValues.Add(column);
                        }
                        else
                        {
                            func.Parameters.Add(column);
                        }

                        break;
                    case MsSqlDbObjectType.P:
                        DbObjectStoredProcedureSchema sp = result.StoredProcedures.FirstOrDefault(f => f.IsSameDbName(dbSchema, objectName));
                        if (sp == null)
                        {
                            throw new InvalidOperationException("Stored procedure not found, but should be present!");
                        }

                        sp.Parameters.Add(column);
                        break;
                    case MsSqlDbObjectType.U:
                        DbObjectTableSchema objectTable = result.Tables.FirstOrDefault(t => t.IsSameDbName(dbSchema, objectName));
                        if (objectTable == null)
                        {
                            objectTable = new DbObjectTableSchema(objectName, dbSchema);
                            result.Tables.Add(objectTable);
                        }

                        objectTable.AddColumn(column);
                        break;
                    default:
                        throw new CaseNotExpectedException(type);
                }
            }
        }

        private static void ReadScriptedObjects(DataTable textTable, Dictionary<long, string> defaultValues, DbObjectCatalogSchema result)
        {
            Dictionary<long, DbObjectTriggerSchema> triggers = new Dictionary<long, DbObjectTriggerSchema>();
            foreach (DataRow? fRow in textTable.Rows)
            {
                if (fRow == null)
                {
                    continue;
                }

                bool isSystemObject = SimpleConvert.Convert<bool>(fRow["IsSystemObject"]);
                if (isSystemObject)
                {
                    continue;
                }

                long objectId = SimpleConvert.Convert<long>(fRow["ObjectId"]);
                string dbSchema = SimpleConvert.Convert<string>(fRow["SchemaName"]);
                string objectName = SimpleConvert.Convert<string>(fRow["ObjectName"]);
                MsSqlDbObjectType type = SimpleConvert.Convert<MsSqlDbObjectType>(fRow["ObjectType"]);
                string text = SimpleConvert.Convert<string>(fRow["Text"]);
                CheckText(objectName, text);
                if (!string.IsNullOrEmpty(text) && text.Contains("DataAnalytics"))
                {
                    Console.WriteLine(objectName);
                }

                switch (type)
                {
                    case MsSqlDbObjectType.D:
                        defaultValues.Add(objectId, text);
                        break;
                    case MsSqlDbObjectType.V:
                        DbObjectViewSchema viewSchema = new DbObjectViewSchema(objectName, dbSchema, text);
                        result.Views.Add(viewSchema);
                        break;
                    case MsSqlDbObjectType.TR:
                    case MsSqlDbObjectType.P:
                    case MsSqlDbObjectType.FN:
                    case MsSqlDbObjectType.TF:
                        DbObjectTriggerSchema scriptedObject;

                        switch (type)
                        {
                            case MsSqlDbObjectType.TR:
                                if (!triggers.ContainsKey(objectId))
                                {
                                    triggers.Add(objectId, new DbObjectTriggerSchema(objectName, objectId, dbSchema));
                                }

                                scriptedObject = triggers[objectId];
                                break;
                            case MsSqlDbObjectType.P:
                                DbObjectStoredProcedureSchema spSchema = result.StoredProcedures.FirstOrDefault(s => s.IsSameDbName(dbSchema, objectName));
                                if (spSchema == null)
                                {
                                    spSchema = new DbObjectStoredProcedureSchema(objectName, objectId, dbSchema);
                                    result.StoredProcedures.Add(spSchema);
                                }

                                scriptedObject = spSchema;
                                break;
                            case MsSqlDbObjectType.FN:
                                DbObjectFunctionSchema fnSchema = result.Functions.FirstOrDefault(s => s.IsSameDbName(dbSchema, objectName));
                                if (fnSchema == null)
                                {
                                    fnSchema = new DbObjectFunctionSchema(objectName, objectId, dbSchema);
                                    result.Functions.Add(fnSchema);
                                }

                                scriptedObject = fnSchema;
                                break;
                            case MsSqlDbObjectType.TF:
                                DbObjectTableValuedFunctionSchema tvfSchema = result.TableValuedFunctions.FirstOrDefault(s => s.IsSameDbName(dbSchema, objectName));
                                if (tvfSchema == null)
                                {
                                    tvfSchema = new DbObjectTableValuedFunctionSchema(objectName, objectId, dbSchema);
                                    result.Functions.Add(tvfSchema);
                                }

                                scriptedObject = tvfSchema;
                                break;
                            default:
                                throw new CaseNotExpectedException(type);
                        }

                        scriptedObject.ScriptText += text;
                        break;
                    default:
                        throw new CaseNotExpectedException(type);
                }
            }
        }

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public DataSet GetDataSetSp(string sp, List<SqlParam> prms)
        {
            return GetDataSetImpl(sp, CommandType.StoredProcedure, prms);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "DataSet should not be IDisposable.")]
        private DataSet GetDataSetImpl(string commandText, CommandType commandType, List<SqlParam>? prms)
        {
            //WriteSqlCommnadToLog(command);

            DataSet result = new DataSet();
            result.Locale = CultureInfo.InvariantCulture;

            using (SqlDataAdapter da = new SqlDataAdapter())
            {
                da.SelectCommand = CreateSqlCommand(commandText, commandType, prms);
                da.Fill(result);
            }

            //WriteExecuteTimeToLog();

            return result;
        }

        //private void ExecuteImpl(string commandText, CommandType commandType, List<SqlParam> prms, int? commandTimeOut = null)
        //{
        //    SqlCommand command = CreateSqlCommand(commandText, commandType, prms, commandTimeOut);

        //    //WriteSqlCommnadToLog(command);
        //    command.ExecuteNonQuery();
        //    //WriteExecuteTimeToLog();

        //    GetReturnedValues(prms);
        //}

        /// <summary>
        /// Returns dataset from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Dataset</returns>
        public void ExecuteSp(string sp, List<SqlParam> prms)
        {
            ExecuteImpl(sp, CommandType.StoredProcedure, prms);
        }

        private void ExecuteImpl(string commandText, CommandType commandType, List<SqlParam> prms)
        {
            SqlCommand command = CreateSqlCommand(commandText, commandType, prms);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns reader from SQL query
        /// </summary>
        /// <param name="query">SQL query</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>Reader</returns>
        public MsSqlDataReader GetDataReader(string query, List<SqlParam>? prms)
        {
            return GetDataReaderImpl(query, CommandType.Text, prms);
        }

        /// <summary>
        /// Returns reader from SQL stored procedure
        /// </summary>
        /// <param name="sp">SQL stored procedure</param>
        /// <param name="prms">SQL parameters</param>
        /// <returns>reader</returns>
        public MsSqlDataReader GetDataReaderSp(string sp, List<SqlParam>? prms)
        {
            return GetDataReaderImpl(sp, CommandType.StoredProcedure, prms);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "DataSet should not be IDisposable.")]
        private MsSqlDataReader GetDataReaderImpl(string commandText, CommandType commandType, List<SqlParam>? prms)
        {
            SqlCommand command = CreateSqlCommand(commandText, commandType, prms);
            return new MsSqlDataReader(command.ExecuteReader());
        }

        /// <summary>
        /// Creates new sql command
        /// </summary>
        /// <param name="commandText">Command text</param>
        /// <param name="commandType">Command type</param>
        /// <param name="prms">Command parameters</param>
        /// <param name="commandTimeOut">Command timeout</param>
        /// <returns>Sql command</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Parameter CommandText is not supouse to be obtained from user.")]
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Method returns IDisposable object.")]
        private SqlCommand CreateSqlCommand(string commandText, CommandType commandType, List<SqlParam>? prms, int? commandTimeOut = null)
        {
            SqlCommand result = new SqlCommand();
            result.Connection = _connection;
            result.Transaction = _transaction;
            result.CommandTimeout = DEFAULT_COMMAND_TIMEOUT_SECONDS;
            if (commandTimeOut.HasValue)
            {
                result.CommandTimeout = commandTimeOut.Value;
            }

            result.CommandText = commandText;
            result.CommandType = commandType;

            AddParamToCommand(result, prms);

            return result;
        }

        private static void AddParamToCommand(SqlCommand command, List<SqlParam>? prms)
        {
            if (prms != null)
            {
                foreach (SqlParam fParam in prms)
                {
                    object value = SimpleConvert.ConvertNullToDbNull(fParam.Value);
                    SqlParameter newpar = command.Parameters.AddWithValue(fParam.SqlName, value);
                    if (fParam.Direction != ParameterDirection.Input)
                    {
                        newpar.Direction = fParam.Direction;

                        //For non-inputed parameter save the original parameter object to retrieve value after SQL execute
                        if (fParam.SqlParameter != null)
                        {
                            throw new InvalidOperationException("Concurency SQL execution call on non-input sql parameter! Not supported!");
                        }

                        fParam.SqlParameter = newpar;
                    }

                    if (fParam.SqlType.HasValue)
                    {
                        newpar.SqlDbType = fParam.SqlType.Value;
                    }

                    if (fParam.Size.HasValue)
                    {
                        newpar.Size = fParam.Size.Value;
                    }
                }
            }
        }

        private static void CheckText(string objName, string text)
        {
            string lower = text.ToLower();
            if (lower.Contains("ps_bydate"))
            {
                Console.WriteLine($"{objName}");
            }
        }
    }
}