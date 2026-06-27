using BBDown.GUI.Models;
using System;
using System.IO;
using System.Text.Json;

namespace BBDown.GUI.Services
{
    public sealed class GuiSettingsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly string _settingsPath;

        public GuiSettingsService()
        {
            _settingsPath = Path.Combine(AppContext.BaseDirectory, "BBDown.GUI.config.json");
        }

        public GuiSettings Load()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    return new GuiSettings();
                }

                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<GuiSettings>(json, JsonOptions) ?? new GuiSettings();
            }
            catch
            {
                return new GuiSettings();
            }
        }

        public void Save(GuiSettings settings)
        {
            var directory = Path.GetDirectoryName(_settingsPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_settingsPath, json);
        }
    }
}
