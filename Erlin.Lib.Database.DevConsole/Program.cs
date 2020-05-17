using System;
using System.Diagnostics;
using System.IO;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Logging;

using Microsoft.Extensions.Configuration;

namespace Erlin.Lib.Database.DevConsole
{
    /// <summary>
    /// Dev test program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point of program
        /// </summary>
        /// <param name="args">Entry args</param>
        public static void Main(string[] args)
        {
            Run(args);
        }

        /// <summary>
        /// Safe entry point
        /// </summary>
        /// <param name="args"></param>
        private static void Run(string[] args)
        {
            try
            {
                Log.LogSystem = new LogMultiplier(new FileLog(TraceLevel.Verbose), new ConsoleLog());

                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                IConfigurationRoot config = new ConfigurationBuilder()
                                            .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                                            .AddJsonFile("appsettings.json", true, true)
                                            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                                            .Build();

                string connString = config.GetConnectionString("drdWebDb");
                DbSchemaGenerator.ReGenerate(connString, @Path.Combine(AppContext.BaseDirectory, "output.sql"));
            }
            finally
            {
                Log.Dispose();
            }
        }
    }
}