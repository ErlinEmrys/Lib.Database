using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Erlin.Lib.Common;
using Erlin.Lib.Common.FileSystem;
using Erlin.Lib.Common.Threading;
using Erlin.Lib.Database.PgSql;

using Microsoft.Extensions.Configuration;

using Npgsql;

namespace Erlin.Lib.Database.PGenesis.WinService
{
    /// <summary>
    /// Postgresql database event listener
    /// </summary>
    public class PGenesisListener
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;
        private readonly string _dbName;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="dbName">Database name</param>
        /// <param name="connectionString">Database connection string</param>
        public PGenesisListener(IConfiguration config, string dbName, string connectionString)
        {
            _config = config;
            _dbName = dbName;
            _connectionString = connectionString;
        }

        /// <summary>
        /// Listen database on selected channels
        /// </summary>
        /// <param name="stoppingToken">Cancellation</param>
        /// <returns></returns>
        public async Task ListenDb(CancellationToken stoppingToken)
        {
            await ParallelHelper.Run(() =>
                                     {
                                         Log.Info($"Listening started for DB: {_dbName}");

                                         while (!stoppingToken.IsCancellationRequested)
                                         {
                                             try
                                             {
                                                 using (PgSqlDbConnect connect = new PgSqlDbConnect(_connectionString))
                                                 {
                                                     connect.Open();

                                                     connect.UnderlyingConnection.Notification += OnNotification;

                                                     connect.Execute($"LISTEN {PgSqlDbConnect.CHANNEL_GENESIS_REQUEST};");
                                                     connect.Execute($"LISTEN {PgSqlDbConnect.CHANNEL_GENESIS_DELETE};");

                                                     while (!stoppingToken.IsCancellationRequested && connect.UnderlyingConnection.State == ConnectionState.Open)
                                                     {
                                                         connect.UnderlyingConnection.Wait(100);
                                                     }
                                                 }
                                             }
                                             catch (Exception e)
                                             {
                                                 Log.Error(e);
                                                 Thread.Sleep(1000);
                                             }
                                         }
                                     });
        }

        /// <summary>
        /// Event on DB notification on listened channels occurs
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="args">Event arguments</param>
        private void OnNotification(object sender, NpgsqlNotificationEventArgs args)
        {
            try
            {
                switch (args.Channel)
                {
                    case PgSqlDbConnect.CHANNEL_GENESIS_REQUEST:

                        if (string.IsNullOrEmpty(args.Payload))
                        {
                            throw new ArgumentException("Payload is empty");
                        }

                        string filePath = Path.Combine(_config["OutputDir"], args.Payload + ".sql");
                        FileHelper.DirectoryEnsure(filePath);

                        string exePath = _config["PgDumpPath"];
                        string argsText = $"{_config["PgDumpArgs"]} --file=\"{filePath}\" {_dbName}";

                        Log.Info($"RUN {exePath} {argsText}");
                        Process pgsql = Process.Start(exePath, argsText);
                        pgsql?.WaitForExit();

                        using (PgSqlDbConnect connect2 = new PgSqlDbConnect(_connectionString))
                        {
                            connect2.Open();
                            connect2.Execute($"SELECT pg_notify('{PgSqlDbConnect.CHANNEL_GENESIS_RESPOND}', '{filePath}'::TEXT);");
                        }

                        break;
                    case PgSqlDbConnect.CHANNEL_GENESIS_DELETE:
                        File.Delete(args.Payload);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"{args.Channel} [{args.Payload}]");
            }
        }
    }
}