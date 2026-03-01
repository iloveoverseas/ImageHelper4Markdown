using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ImageHelper4Markdown
{
    public class Clipboard2Png
    {
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        public string SaveDirectory { get; set; }
        public event Action<string> OnImageSaved;
        public event Action<Exception> OnError;

        private readonly IntPtr _hwnd;
        private readonly Window _ownerWindow;

        public Clipboard2Png(Window window)
        {
            _hwnd = new WindowInteropHelper(window).Handle;
            _ownerWindow = window;
        }

        public bool IsMonitoring { get; private set; } = false;

        /// <summary>
        /// クリップボード監視を開始します
        /// </summary>
        public void StartMonitoring()
        {
            if (IsMonitoring)
            {
                return;
            }
            AddClipboardFormatListener(_hwnd);
            HwndSource.FromHwnd(_hwnd).AddHook(HwndHandler);
            IsMonitoring = true;
        }

        /// <summary>
        /// クリップボード監視を停止します
        /// </summary>
        public void StopMonitoring()
        {
            if (IsMonitoring == false)
            {
                return;
            }
            RemoveClipboardFormatListener(_hwnd);
            HwndSource.FromHwnd(_hwnd).RemoveHook(HwndHandler);
            IsMonitoring = false;
        }

        public bool IsCatchImageInClipboard { get; set; }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                // 画像が含まれている場合のみ処理
                if (Clipboard.ContainsImage())
                {
                    if (IsCatchImageInClipboard == true)
                    {
                        return IntPtr.Zero;
                    }

                    IsCatchImageInClipboard = true;

                    ExecuteSave();

                    StopMonitoring();
                    IsCatchImageInClipboard = false;

                }
            }
            return IntPtr.Zero;
        }

        private void ExecuteSave()
        {
            try
            {
                BitmapSource bitmap = Clipboard.GetImage();
                if (bitmap == null)
                {
                    return;
                }

                if (Directory.Exists(SaveDirectory) == false)
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                // ダイアログで説明とファイル名を入力
                var dialog = new ImageSaveInfoDialog();
                dialog.Owner = _ownerWindow;
                dialog.FileName = $"Clipboard2Png_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                string description = dialog.Description?.Trim();
                string pngFileName = dialog.FileName?.Trim();


                string filePath = Path.Combine(SaveDirectory, pngFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }

                // Markdown文字列を生成
                string markdown = $"![{description}]({pngFileName})";
                Clipboard.SetText(markdown);

                OnImageSaved?.Invoke(filePath);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
        }
    }
}