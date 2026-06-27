using BBDown.GUI.Models;
using System.Collections.ObjectModel;

namespace BBDown.GUI.ViewModels
{
    public sealed class TaskQueueViewModel : ViewModelBase
    {
        public ObservableCollection<DownloadJob> Jobs { get; } = new();
    }
}
