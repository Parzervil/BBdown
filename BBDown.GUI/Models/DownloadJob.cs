using BBDown;
using BBDown.GUI.ViewModels;
using System;

namespace BBDown.GUI.Models
{
    public sealed class DownloadJob : ViewModelBase
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");

        private string _url = "";
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private string _title = "";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private DownloadJobStatus _status = DownloadJobStatus.Queued;
        public DownloadJobStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _speedText = "";
        public string SpeedText
        {
            get => _speedText;
            set => SetProperty(ref _speedText, value);
        }

        private string _outputPath = "";
        public string OutputPath
        {
            get => _outputPath;
            set => SetProperty(ref _outputPath, value);
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public MyOption Option { get; set; } = new();
    }
}
