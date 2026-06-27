using BBDown.Core;
using System;

namespace BBDown.GUI.Models
{
    public class LogEntry
    {
        public DateTime Time { get; set; }
        public LogLevel Level { get; set; }
        public string Message { get; set; } = "";
    }
}
