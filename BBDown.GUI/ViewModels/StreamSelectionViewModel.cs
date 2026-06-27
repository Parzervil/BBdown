using BBDown.GUI.Models;
using System.Collections.ObjectModel;

namespace BBDown.GUI.ViewModels
{
    public sealed class StreamSelectionViewModel : ViewModelBase
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _statusText = "解析后会自动展示第一个分P的可用流。";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public ObservableCollection<VideoStreamOption> VideoStreams { get; } = new();
        public ObservableCollection<AudioStreamOption> AudioStreams { get; } = new();

        public void Clear(string statusText)
        {
            VideoStreams.Clear();
            AudioStreams.Clear();
            StatusText = statusText;
        }
    }
}
