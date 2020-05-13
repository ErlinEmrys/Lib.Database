using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Erlin.Lib.Common.FileSystem;
using Erlin.Lib.Database.MsSql;
using Erlin.Lib.Database.MsSql.Schema;
using Erlin.Lib.Database.Schema;

namespace Erlin.Lib.Database
{
    /// <summary>
    /// Db schema generator
    /// </summary>
    public static class DbSchemaGenerator
    {
        /// <summary>
        /// ReGenerate db schema
        /// </summary>
        /// <param name="connString">Connection string to database</param>
        /// <param name="outputFilePath">Path to output file</param>
        public static void ReGenerate(string connString, string outputFilePath)
        {
            using (MsSqlDbConnect connect = new MsSqlDbConnect(connString))
            {
                connect.Open();
                MsSqlDbSchema dbSchema = connect.ReadSchema();
                StringBuilder createScript = dbSchema.GenerateCreateScript();
                FileHelper.WriteAllText(outputFilePath, createScript.ToString());
            }
        }
    }
}
