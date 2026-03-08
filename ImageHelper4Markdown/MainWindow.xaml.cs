using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageHelper4Markdown
{
    public partial class MainWindow : Window
    {
        private Clipboard2Png _SaveClipboardImage;

        public MainWindow()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (sender, e) => this.DragMove();             // キャプションバー以外でもドラッグ可能にする

            this.Topmost = true;
            this.Topmost = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _SaveClipboardImage = new Clipboard2Png();
            _SaveClipboardImage.SaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);      // デフォルトはデスクトップ
        }

        private void BtnCapture_Click(object sender, RoutedEventArgs e)
        {
            // Snipping Tool 起動
            Process.Start("explorer.exe", "ms-screenclip:");

            // SnippingTool.exe が起動するまで待つ
            Process snip = null;
            while (snip == null)
            {
                snip = Process.GetProcessesByName("SnippingTool").FirstOrDefault();
                Thread.Sleep(50);
            }

            snip.WaitForExit();                                     // Snipping Tool の終了を待つ

            _SaveClipboardImage.Save();                             // クリップボードの画像を保尊
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog("フォルダーの選択");
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _SaveClipboardImage.SaveFolder = dialog.FileName;
            }
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
