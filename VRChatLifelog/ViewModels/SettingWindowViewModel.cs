using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using VRChatLifelog.Models;

namespace VRChatLifelog.ViewModels
{
    public class SettingWindowViewModel : ViewModel
    {
        // Some useful code snippets for ViewModel are defined as l*(llcom, llcomn, lvcomm, lsprop, etc...).

        // This method would be called from View, when ContentRendered event was raised.
        public void Initialize()
        {
            LoadSettings();
        }

        /// <summary>
        /// Windowsの起動時にアプリを開始するか
        /// </summary>
        public bool IsRunAtStartup
        {
            get => _isRunAtStartup;
            set => RaisePropertyChangedIfSet(ref _isRunAtStartup, value);
        }
        private bool _isRunAtStartup;

        /// <summary>
        /// 変更を保存して画面を閉じます．
        /// </summary>
        public void OkClose()
        {
            SaveSettings();
            CloseWindow();
        }
        private ViewModelCommand? _OkCloseCommand;
        public ViewModelCommand OkCloseCommand => _OkCloseCommand ??= new ViewModelCommand(OkClose);

        /// <summary>
        /// 変更を破棄して画面を閉じます．
        /// </summary>
        public void CancelClose()
        {
            var msg = new ConfirmationMessage("変更は破棄されます．よろしいですか？", "確認", MessageBoxImage.Information, MessageBoxButton.OKCancel, "Confirm");
            if (Messenger.GetResponse(msg)?.Response is true)
            {
                CloseWindow();
            }
        }
        private ViewModelCommand? _CancelCloseCommand;
        public ViewModelCommand CancelCloseCommand => _CancelCloseCommand ??= new ViewModelCommand(CancelClose);

        private void CloseWindow()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "WindowAction"));
        }

        /// <summary>
        /// 設定を読み込みます．
        /// </summary>
        private void LoadSettings()
        {
            IsRunAtStartup = Properties.Settings.Default.IsRunAtStartup;
        }

        /// <summary>
        /// 設定を保存します．
        /// </summary>
        private void SaveSettings()
        {
            Properties.Settings.Default.IsRunAtStartup = IsRunAtStartup;
            if (IsRunAtStartup)
            {
                StartupRegister.Register();
            }
            else
            {
                StartupRegister.Unregister();
            }

            Properties.Settings.Default.Save();
        }
    }
}
