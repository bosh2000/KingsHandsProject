namespace KingsHandsProject.Models
{
    public sealed class ScanResult
    {
        public List<PokerHand> Hands { get; } = new();
        public int ProcessedFilesCount { get; set; }
        public int SkippedFilesCount { get; set; }
        public List<string> Errors { get; } = new();
    }
}