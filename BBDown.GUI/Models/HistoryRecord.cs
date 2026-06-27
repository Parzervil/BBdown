using System;

namespace BBDown.GUI.Models
{
    public sealed class HistoryRecord
    {
        public DateTime FinishedAt { get; set; } = DateTime.Now;
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string OutputPath { get; set; } = "";
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; } = "";
    }
}
