using KingsHandsProject.Models;

namespace KingsHandsProject.Services.Interfaces
{
    public interface IPokerLogScannerService
    {
        ScanResult Scan(string rootFolder, Action<ScanProgress>? progressCallback = null);
    }
}