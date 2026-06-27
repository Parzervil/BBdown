using BBDown.GUI.Commands;
using BBDown.GUI.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace BBDown.GUI.ViewModels
{
    public sealed class HistoryViewModel : ViewModelBase
    {
        private readonly Action<HistoryRecord> _retry;

        public HistoryViewModel(Action<HistoryRecord> retry)
        {
            _retry = retry;
            OpenDirectoryCommand = new RelayCommand(_ => OpenDirectory(), _ => SelectedRecord != null);
            RetryCommand = new RelayCommand(_ => RetrySelected(), _ => SelectedRecord != null);
            DeleteCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedRecord != null);
            ClearCommand = new RelayCommand(_ => Records.Clear(), _ => Records.Count > 0);
        }

        public ObservableCollection<HistoryRecord> Records { get; } = new();

        private HistoryRecord? _selectedRecord;
        public HistoryRecord? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (SetProperty(ref _selectedRecord, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand OpenDirectoryCommand { get; }
        public ICommand RetryCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCommand { get; }

        public void Add(HistoryRecord record)
        {
            Records.Insert(0, record);
        }

        public void LoadFrom(System.Collections.Generic.IEnumerable<HistoryRecord> records)
        {
            Records.Clear();
            foreach (var record in records)
            {
                Records.Add(record);
            }
        }

        private void OpenDirectory()
        {
            if (SelectedRecord == null || string.IsNullOrWhiteSpace(SelectedRecord.OutputPath))
            {
                return;
            }

            var path = SelectedRecord.OutputPath;
            if (!Directory.Exists(path))
            {
                path = Path.GetDirectoryName(path) ?? "";
            }

            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = path,
                UseShellExecute = true
            });
        }

        private void RetrySelected()
        {
            if (SelectedRecord != null)
            {
                _retry(SelectedRecord);
            }
        }

        private void DeleteSelected()
        {
            if (SelectedRecord == null)
            {
                return;
            }

            var record = SelectedRecord;
            SelectedRecord = null;
            Records.Remove(record);
        }
    }
}
