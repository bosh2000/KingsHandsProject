using KingsHandsProject.Infrastructure;
using KingsHandsProject.Models;
using KingsHandsProject.Services.Interfaces;
using System.IO;

namespace KingsHandsProject.Services
{
    public sealed class PokerLogScannerService : IPokerLogScannerService
    {
        private readonly IPokerHandParser _parser;

        public PokerLogScannerService(IPokerHandParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public ScanResult Scan(string rootFolder, Action<ScanProgress>? progressCallback = null)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
                throw new ArgumentException("Root folder is null or empty.", nameof(rootFolder));

            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException($"Directory not found: {rootFolder}");

            ScanResult result = new();

            string[] files;
            try
            {
                files = Directory.GetFiles(rootFolder, "*.json", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                DebugLogger.LogException(ex);
                throw;
            }

            int totalFiles = files.Length;

            for (int i = 0; i < totalFiles; i++)
            {
                string filePath = files[i];

                try
                {
                    IReadOnlyList<PokerHand> hands = _parser.ParseFile(filePath);

                    result.Hands.AddRange(hands);
                    result.ProcessedFilesCount++;
                }
                catch (Exception ex) when (
                    ex is IOException ||
                    ex is UnauthorizedAccessException ||
                    ex is System.Text.Json.JsonException ||
                    ex is NotSupportedException)
                {
                    result.SkippedFilesCount++;
                    result.Errors.Add($"{filePath}: {ex.Message}");

                    DebugLogger.LogError($"Failed to process file: {filePath}");
                    DebugLogger.LogException(ex);
                }

                progressCallback?.Invoke(new ScanProgress
                {
                    ProcessedFiles = i + 1,
                    TotalFiles = totalFiles,
                    CurrentFile = filePath
                });
            }

            return result;
        }
    }
}