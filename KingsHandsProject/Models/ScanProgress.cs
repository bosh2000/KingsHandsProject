namespace KingsHandsProject.Models
{
    public sealed class ScanProgress

    {
        public int ProcessedFiles { get; set; }
        public int TotalFiles { get; set; }
        public string? CurrentFile { get; set; }
    }
}