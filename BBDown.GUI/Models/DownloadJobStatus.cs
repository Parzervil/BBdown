namespace BBDown.GUI.Models
{
    public enum DownloadJobStatus
    {
        Queued,
        Parsing,
        Downloading,
        Muxing,
        Completed,
        Failed,
        Canceled
    }
}
