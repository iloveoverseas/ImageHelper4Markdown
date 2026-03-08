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

        // クリップボード監視の状態を示すプロパティ
        public bool IsMonitoring { get; private set; } = false;

        public void StartMonitoring()           // クリップボード監視を開始
        {
            if (IsMonitoring)
            {
                return;
            }
            AddClipboardFormatListener(_hwnd);
            HwndSource.FromHwnd(_hwnd).AddHook(HwndHandler);
            IsMonitoring = true;
        }

        public void StopMonitoring()            // クリップボード監視を停止
        {
            if (IsMonitoring == false)
            {
                return;
            }
            RemoveClipboardFormatListener(_hwnd);
            HwndSource.FromHwnd(_hwnd).RemoveHook(HwndHandler);
            IsMonitoring = false;
        }

        // クリップボードに画像が含まれている場合の処理中フラグ。これにより、連続でキャプチャしている場合など、処理中にさらにクリップボードが更新される可能性があるため、二重処理を防止する。
        public bool IsCatchImageInClipboard { get; set; }

        private IntPtr HwndHandler(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                if (Clipboard.ContainsImage())                  // クリップボードに画像が含まれているか確認
                {
                    if (IsCatchImageInClipboard == true)        // 連続でキャプチャしている場合など、処理中にさらにクリップボードが更新される可能性があるため、二重処理を防止する
                    {
                        return IntPtr.Zero;                     // すでに処理中であれば、何もしない
                    }

                    IsCatchImageInClipboard = true;

                    ExecuteSavePngFromClipboard();              // クリップボードからPNG保存処理を実行

                    StopMonitoring();                           // 画像を保存した後は、監視を停止して二重処理を防止する

                    IsCatchImageInClipboard = false;
                }
            }

            return IntPtr.Zero;
        }

        private void ExecuteSavePngFromClipboard()
        {
            try
            {
                BitmapSource bitmap = Clipboard.GetImage();     // クリップボードから画像を取得
                if (bitmap == null)
                {
                    return;
                }

                if (Directory.Exists(SaveDirectory) == false)   // 保存先ディレクトリが存在しない場合は作成
                {
                    Directory.CreateDirectory(SaveDirectory);
                }

                // ダイアログで説明とファイル名を入力
                var dialog = new ImageSaveInfoDialog(_ownerWindow);
                //dialog.Owner = _ownerWindow;
                dialog.FileName = $"Clipboard2Png_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                if (dialog.ShowDialog() != true)
                {
                    return;
                }
                dialog.Close();

                string description = dialog.Description?.Trim();
                string pngFileName = dialog.FileName?.Trim();

                string filePath = Path.Combine(SaveDirectory, pngFileName);

                // PNGファイルとして保存
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }

                // Markdown文字列を生成
                string markdown = $"![{description}]({pngFileName})";
                Clipboard.SetText(markdown);

                OnImageSaved?.Invoke(filePath);     // 画像が保存された場合は、OnImageSavedイベントを通じて保存されたファイルパスを通知
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);    // エラーが発生した場合は、OnErrorイベントを通じて通知
            }
        }
    }
}