using BBDown.GUI.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BBDown.GUI.ViewModels
{
    public sealed class ServiceViewModel : ViewModelBase
    {
        private BBDownApiServer? _server;

        public ServiceViewModel()
        {
            StartCommand = new RelayCommand(_ => Start(), _ => !IsRunning);
            StopCommand = new RelayCommand(_ => Stop(), _ => IsRunning);
        }

        private string _listenUrl = "http://127.0.0.1:23333";
        public string ListenUrl
        {
            get => _listenUrl;
            set => SetProperty(ref _listenUrl, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string _statusText = "服务未启动。";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        private void Start()
        {
            if (!Uri.TryCreate(ListenUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttp)
            {
                StatusText = "请输入合法的 HTTP 监听地址，例如 http://127.0.0.1:23333";
                return;
            }

            _server = new BBDownApiServer();
            _server.SetUpServer();
            IsRunning = true;
            StatusText = $"服务启动中：{ListenUrl}";

            Task.Run(() =>
            {
                try
                {
                    _server.Run(ListenUrl);
                }
                catch (Exception ex)
                {
                    IsRunning = false;
                    StatusText = $"服务已停止：{ex.Message}";
                }
            });
        }

        private void Stop()
        {
            StatusText = "当前底层服务未暴露停止接口；请关闭 GUI 结束服务进程。";
        }
    }
}
