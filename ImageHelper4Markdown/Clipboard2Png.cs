using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ImageHelper4Markdown
{
    public class Clipboard2Png
    {
        public string SaveFolder { get; set; }

        public Clipboard2Png()
        {
        }

        public void Save()
        {
            if (Clipboard.ContainsImage())
            {
                BitmapSource bitmap = Clipboard.GetImage();
                if (bitmap == null)
                {
                    return;
                }

                // ダイアログで説明とファイル名を入力
                var dialog = new ImageSaveInfoDialog();
                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                string description = dialog.Description;
                string pngFileName = description + ".png";
                string filePath = Path.Combine(SaveFolder, pngFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }

                // Markdown文字列を生成
                string markdown = $"![{description}]({pngFileName})";
                Clipboard.SetText(markdown);
            }
        }
    }
}