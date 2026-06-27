using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BBDown.GUI.Views
{
    public partial class QrCodeWindow : Window
    {
        private readonly CancellationTokenSource _cts;
        private bool _closedByCode;

        public QrCodeWindow(string title, CancellationTokenSource cts)
        {
            InitializeComponent();
            Title = title;
            _cts = cts;
            StatusTextBlock.Text = "正在生成二维码...";
            Loaded += OnLoaded;
        }

        public void UpdateStatus(string status)
        {
            Dispatcher.Invoke(() => StatusTextBlock.Text = status);
        }

        public void SetQrCode(string filePath)
        {
            Dispatcher.Invoke(() =>
            {
                if (!File.Exists(filePath)) return;

                try
                {
                    var bitmap = new BitmapImage();
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    QrCodeImage.Source = bitmap;
                    StatusTextBlock.Text = "请使用哔哩哔哩客户端扫描";
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"加载二维码失败: {ex.Message}";
                }
            });
        }

        public void CloseSafely()
        {
            _closedByCode = true;
            Dispatcher.Invoke(Close);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Closed += (_, _) =>
            {
                if (!_closedByCode)
                {
                    _cts.Cancel();
                }
            };
        }
    }
}
