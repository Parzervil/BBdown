namespace BBDown.GUI.Models
{
    public sealed class GuiSettings
    {
        public string OutputDir { get; set; } = "";
        public string ApiMode { get; set; } = "WEB";
        public string DownloadMode { get; set; } = "Full";
        public string EncodingPriority { get; set; } = "hevc,av1,avc";
        public string DfnPriority { get; set; } = "8K 超高清,4K 超清,1080P 高码率,1080P 高清";
        public string FilePattern { get; set; } = "<videoTitle>";
        public string MultiFilePattern { get; set; } = "<videoTitle>/[P<pageNumberWithZero>]<pageTitle>";
        public bool DownloadDanmaku { get; set; }
        public bool SkipSubtitle { get; set; }
        public bool SkipCover { get; set; }
        public bool SkipMux { get; set; }
        public bool UseAria2c { get; set; }
        public bool MultiThread { get; set; } = true;
        public bool UseMp4box { get; set; }
        public bool ShowAll { get; set; }
        public bool Debug { get; set; }
    }
}
