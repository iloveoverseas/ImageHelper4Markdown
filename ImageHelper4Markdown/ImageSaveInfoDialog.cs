using System;
using System.Windows;

namespace ImageHelper4Markdown
{
    public partial class ImageSaveInfoDialog : Window
    {
        public string Description
        {
            get => DescriptionBox.Text;
            set => DescriptionBox.Text = value;
        }

        public string FileName
        {
            get => FileNameBox.Text;
            set => FileNameBox.Text = value;
        }

        public ImageSaveInfoDialog(Window owner)
        {
            InitializeComponent();

            this.Owner = owner;

            EnableNonTitleBarDrag();
        }

        private void EnableNonTitleBarDrag()
        {
            // キャプションバー以外でもドラッグ可能にする
            this.MouseLeftButtonDown += (sender, e) => this.DragMove();

            this.Topmost = true;
            this.Topmost = false;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionBox.Text) || string.IsNullOrWhiteSpace(FileNameBox.Text))
            {
                return;
            }

            if (FileNameBox.Text.EndsWith(".png", StringComparison.OrdinalIgnoreCase) == false)
            {
                return;
            }

            DialogResult = true;
        }
    }
}