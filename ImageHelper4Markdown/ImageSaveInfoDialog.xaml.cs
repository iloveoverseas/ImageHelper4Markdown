using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ImageHelper4Markdown
{
    public partial class ImageSaveInfoDialog : Window
    {
        public string Description;

        public ImageSaveInfoDialog()
        {
            InitializeComponent();

            this.Topmost = true;
            this.Top = -500;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // 1. 横中央に配置
            // Window が表示されるモニターを取得
            var screen = System.Windows.Forms.Screen.FromHandle(
                new System.Windows.Interop.WindowInteropHelper(this).Handle);

            // 横方向の中央に配置
            this.Left = screen.WorkingArea.Left + (screen.WorkingArea.Width - this.Width) / 2;

            // 2. 上からスライドイン
            double startTop = -220;
            double endTop = 0;

            // 初期位置を上にずらす
            this.Top = startTop;

            // スライドインアニメーション
            var animation = new DoubleAnimation
            {
                From = startTop,
                To = endTop,
                Duration = TimeSpan.FromSeconds(1.5), // ← 1秒でスライドイン
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            this.BeginAnimation(Window.TopProperty, animation);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                Message.Background = Brushes.DarkRed;
                Message.Text = "ファイル名が入力されていません。";
                return;
            }

            Description = DescriptionBox.Text.Trim();

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DescriptionBox.Focus();
        }
    }
}