using Livet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VRChatLifelog.Data;
using VRChatLifelog.Services;

namespace VRChatLifelog
{
    public partial class App : Application
    {
        private static readonly IHost _host = CreateHostBuilder().Build();

        private ILogger<App>? _logger;

        /// <summary>
        /// データディレクトリ
        /// </summary>
        public static string DataDirectory { get; } = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VRChatLifelog"
        );

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
                    services.AddSingleton<LogWatcherService>();
                    services.AddHostedService(p => p.GetRequiredService<LogWatcherService>());
                    services.AddOptions<LogWatchOption>();
                    services.AddHostedService<NotifyIconService>();
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

        protected override void OnExit(ExitEventArgs e)
        {
            _host.StopAsync(TimeSpan.FromSeconds(5)).Wait();
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

        private void InitDb()
        {
            _logger?.LogInformation("Initializing database...");

            var context = _host.Services.GetRequiredService<LifelogContext>();
            var dir = Path.GetDirectoryName(context.DbPath);

            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }

            context.Database.Migrate();

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

            int historyId = 1;
            foreach (var history in context.JoinLeaveHistories.OrderBy(h => h.Joined))
            {
                history.Id = historyId++;
            }

            // 場所履歴とJoinLeave履歴の関連付け
            DateTime lastLeft = DateTime.MinValue;
            foreach (var location in context.LocationHistories.OrderBy(h => h.Id))
            {
                var nextJoin = context.LocationHistories.FirstOrDefault(h => h.Id == location.Id + 1)?.Joined ?? DateTime.MaxValue;
                await context.JoinLeaveHistories
                    .Where(h => lastLeft <= h.Joined && h.Left < nextJoin)
                    .ForEachAsync(h => h.LocationHistoryId = location.Id);
                lastLeft = location.Left!.Value;
            }
        }
    }
}
