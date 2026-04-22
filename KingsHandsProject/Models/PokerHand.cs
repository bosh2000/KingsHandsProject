namespace KingsHandsProject.Models
{
    /// <summary>
    /// Одна запись из json.
    /// </summary>
    public sealed class PokerHand
    {
        public long HandId { get; init; }
        public string TableName { get; init; } = string.Empty;
        public IReadOnlyList<string> Players { get; init; } = new List<string>();
        public IReadOnlyList<string> Winners { get; init; } = new List<string>();
        public string WinAmount { get; init; } = string.Empty;
    }
}