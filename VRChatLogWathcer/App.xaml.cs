using Livet;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using VRChatLogWathcer.Models;

namespace VRChatLogWathcer
{
    public partial class App : Application
    {
        private NotifyIconWrapper? _notifyIconWrapper;

        [STAThread]
        public static void Main()
        {
            App app = new();
            app.InitializeComponent();
            app.Run();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DispatcherHelper.UIDispatcher = Dispatcher;
            //AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            InitDb();
            _notifyIconWrapper = new NotifyIconWrapper();

            var wathcer = new LogWathcer(@"C:\Users\rio\AppData\LocalLow\VRChat\VRChat");
            await wathcer.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _notifyIconWrapper?.Dispose();
        }

        // Application level error handling
        //private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        //{
        //    //TODO: Logging
        //    MessageBox.Show(
        //        "Something errors were occurred.",
        //        "Error",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Error);
        //
        //    Environment.Exit(1);
        //}

        private void InitDb()
        {
            using var context = new LifelogContext();
            var dir = Path.GetDirectoryName(context.DbPath);

            if (dir is not null)
            {
                Directory.CreateDirectory(dir);
            }

            context.Database.Migrate();
        }
    }
}
