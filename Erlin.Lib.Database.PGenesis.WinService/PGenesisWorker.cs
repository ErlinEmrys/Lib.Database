using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Erlin.Lib.Common;
using Erlin.Lib.Database.PgSql;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Erlin.Lib.Database.PGenesis.WinService
{
    /// <summary>
    /// PostgreSql database create script help service
    /// </summary>
    public class PGenesisWorker : BackgroundService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="config">Configuration</param>
        public PGenesisWorker(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Start of a service
        /// </summary>
        /// <param name="cancellationToken">Cancellation</param>
        /// <returns></returns>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // DO YOUR STUFF HERE
            await base.StartAsync(cancellationToken);

            Log.Info("PGenesisWorker.Started!");
        }

        /// <summary>
        /// Stop of a service
        /// </summary>
        /// <param name="cancellationToken">Cancellation</param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // DO YOUR STUFF HERE
            await base.StopAsync(cancellationToken);

            Log.Info("PGenesisWorker.Stopped!");
        }

        /// <summary>
        /// Execution of a service
        /// </summary>
        /// <param name="stoppingToken">Cancellation</param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("PGenesisWorker.Execute!");

            string connString = GetConnectionString(_config["PgDefaultDb"]);
            List<string> databases = new List<string>();

            using (PgSqlDbConnect connect = new PgSqlDbConnect(connString))
            {
                connect.Open();
                using (PgSqlDataReader reader = connect.GetDataReader("SELECT datname FROM pg_database WHERE datistemplate = false AND datname != 'postgres';"))
                {
                    while (reader.UnderlyingReader.Read())
                    {
                        databases.Add(reader.ReadString("datname"));
                    }
                }
            }

            List<Task> allTasks = new List<Task>();
            foreach (string fDbName in databases)
            {
                PGenesisListener fListener = new PGenesisListener(_config, fDbName, GetConnectionString(fDbName));
                Task fTask = fListener.ListenDb(stoppingToken);
                allTasks.Add(fTask);
            }

            Task.WaitAll(allTasks.ToArray(), stoppingToken);

            Log.Info("PGenesisWorker.Execute finished!");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dynamic databse connection string
        /// </summary>
        private string GetConnectionString(string dbName)
        {
            return
                $"Server={Environment.GetEnvironmentVariable("PGHOST")};Port={Environment.GetEnvironmentVariable("PGPORT")};User Id={Environment.GetEnvironmentVariable("PGUSER")};Password={Environment.GetEnvironmentVariable("PGPASSWORD")};Database={dbName};";
        }

        /// <summary>
        /// Dispose of a service
        /// </summary>
        public override void Dispose()
        {
            // DO YOUR STUFF HERE
            Log.Info("PGenesisWorker.Dispose!");
        }
    }
}