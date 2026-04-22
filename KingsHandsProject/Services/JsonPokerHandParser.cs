using KingsHandsProject.Infrastructure;
using KingsHandsProject.Models;
using KingsHandsProject.Services.Interfaces;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KingsHandsProject.Services
{
    public sealed class JsonPokerHandParser : IPokerHandParser
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public IReadOnlyList<PokerHand> ParseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is null or empty.", nameof(filePath));

            string json = File.ReadAllText(filePath);

            List<PokerHandDto>? dtoItems = JsonSerializer.Deserialize<List<PokerHandDto>>(json, JsonOptions);

            List<PokerHand> result = new();

            if (dtoItems is null)
                return result;

            foreach (PokerHandDto dto in dtoItems)
            {
                if (!IsValid(dto))
                {
                    DebugLogger.LogError($"Skipped invalid hand entry in file: {filePath}");
                    continue;
                }

                PokerHand hand = new PokerHand
                {
                    HandId = dto.HandId!.Value,
                    TableName = dto.TableName!.Trim(),
                    Players = dto.Players ?? new List<string>(),
                    Winners = dto.Winners ?? new List<string>(),
                    WinAmount = dto.WinAmount?.Trim() ?? string.Empty
                };

                result.Add(hand);
            }

            return result;
        }

        private static bool IsValid(PokerHandDto dto)
        {
            return dto.HandId.HasValue
                   && !string.IsNullOrWhiteSpace(dto.TableName);
        }

        private sealed class PokerHandDto
        {
            [JsonPropertyName("HandID")]
            public long? HandId { get; set; }

            [JsonPropertyName("TableName")]
            public string? TableName { get; set; }

            [JsonPropertyName("Players")]
            public List<string>? Players { get; set; }

            [JsonPropertyName("Winners")]
            public List<string>? Winners { get; set; }

            [JsonPropertyName("WinAmount")]
            public string? WinAmount { get; set; }
        }
    }
}