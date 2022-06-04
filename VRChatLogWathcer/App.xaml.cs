﻿using Livet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VRChatLogWathcer.Models;
using VRChatLogWathcer.Views;

namespace VRChatLogWathcer
{
    public partial class App : Application
    {
        private static readonly IHost _host = CreateHostBuilder().Build();

        private ILogger<App>? _logger;

        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.Run();
        }

        public static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddDbContext<LifelogContext>();
                    services.AddHostedService<LogWathcerService>();
                    services.AddOptions<LogWatchOption>();
                    services.AddHostedService<NotifyIconService>();

                    services.AddSingleton<MainWindow>();
                })
                .ConfigureHostConfiguration(config =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", true, true);
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddNLog();
                })
                .UseConsoleLifetime(options => options.SuppressStatusMessages = true);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            _logger = _host.Services.GetRequiredService<ILogger<App>>();

            InitDb();

            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            DispatcherHelper.UIDispatcher = Dispatcher;

            await _host.StartAsync();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();

            base.OnExit(e);
        }

        // Application level error handling
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Something errors were occurred.\n{e.ExceptionObject}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            try
            {
                _logger?.LogError(e.ExceptionObject as Exception, "CurrentDomain_UnhandledException");
            }
            catch { }

            Environment.Exit(1);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Something errors were occurred.\n{e.Exception}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            try
            {
                _logger?.LogError(e.Exception, "Dispatcher_UnhandledException");
            }
            catch { }

            Environment.Exit(1);
        }

        private async void InitDb()
        {
            _logger?.LogInformation("Initializing database...");

            var context = _host.Services.GetRequiredService<LifelogContext>();
            var dir = Path.GetDirectoryName(context.DbPath);

            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }

            context.Database.Migrate();
            await MigrateDbData(context);

            _logger?.LogInformation("Database initialization completed");
        }

        private async Task MigrateDbData(LifelogContext context)
        {
            // IDを振る
            int locationId = 1;
            foreach (var location in context.LocationHistories.OrderBy(h => h.Joined))
            {
                location.Id = locationId++;
            }

            // 場所履歴とJoinLeave履歴の関連付け
            DateTime lastLeft = DateTime.MinValue;
            foreach (var location in context.LocationHistories.OrderBy(h => h.Id))
            {
                var nextJoin = context.LocationHistories.FirstOrDefault(h => h.Id == location.Id + 1)?.Joined ?? DateTime.MaxValue;
                await context.JoinLeaveHistories
                    .Where(h => lastLeft <= h.Joined && h.Left < nextJoin)
                    .ForEachAsync(h => h.LocaionId = location.Id);
                lastLeft = location.Left!.Value;
            }
        }
    }
}
