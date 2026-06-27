using BBDown.GUI.Models;
using System.Collections.ObjectModel;

namespace BBDown.GUI.ViewModels
{
    public sealed class LogViewModel : ViewModelBase
    {
        public ObservableCollection<LogEntry> Logs { get; } = new();
    }
}
