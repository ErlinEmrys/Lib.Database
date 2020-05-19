using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Erlin.Lib.Common;
using Erlin.Lib.Common.Logging;
using Erlin.Lib.Common.Reflection;

namespace Erlin.Lib.Database.PGenesis.WinService
{
    /// <summary>
    /// Main program
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
        /// <param name="args">Entry args</param>
        private static void Run(string[] args)
        {
            try
            {
                Console.WriteLine(Path.Combine(AssemblyHelper.BaseLocation, "Log"));
                Log.LogSystem = new LogMultiplier(new FileLog(TraceLevel.Verbose), new ConsoleLog());

                CreateHostBuilder(args).Build().Run();
            }
            finally
            {
                Log.Dispose();
            }
        }

        /// <summary>
        /// Host creation
        /// </summary>
        /// <param name="args">Entry args</param>
        /// <returns>Host</returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .UseWindowsService()
                       .ConfigureServices((hostContext, services) => services.AddHostedService<PGenesisWorker>());
        }
    }
}