using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageHelper4Markdown
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private Clipboard2Png _saver;

        public MainWindow()
        {
            InitializeComponent();

            EnableNonTitleBarDrag();
        }

        private void EnableNonTitleBarDrag()
        {
            // キャプションバー以外でもドラッグ可能にする
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();

            this.Topmost = true;
            this.Topmost = false;
        }

        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (_saver.IsMonitoring == false)
            {
                _saver.StartMonitoring();
            }
            Process.Start("explorer.exe", "ms-screenclip:"); // Windows 11 では Snipping Tool が統合され、URI スキームでキャプチャを直接開始できます。
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog("フォルダーの選択");
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _saver.SaveDirectory = dialog.FileName;
            }
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _saver = new Clipboard2Png(this);
            _saver.SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // デフォルトはデスクトップ
        }
    }
}
