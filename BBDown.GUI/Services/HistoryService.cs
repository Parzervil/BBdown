using BBDown.GUI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace BBDown.GUI.Services
{
    public sealed class HistoryService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly string _historyPath;

        public HistoryService()
        {
            _historyPath = Path.Combine(AppContext.BaseDirectory, "BBDown.GUI.history.json");
        }

        public IReadOnlyList<HistoryRecord> Load()
        {
            try
            {
                if (!File.Exists(_historyPath))
                {
                    return [];
                }

                var json = File.ReadAllText(_historyPath);
                return JsonSerializer.Deserialize<List<HistoryRecord>>(json, JsonOptions) ?? [];
            }
            catch
            {
                return [];
            }
        }

        public void Save(IEnumerable<HistoryRecord> records)
        {
            var directory = Path.GetDirectoryName(_historyPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(records, JsonOptions);
            File.WriteAllText(_historyPath, json);
        }
    }
}
