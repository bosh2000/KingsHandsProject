using KingsHandsProject.Models;

namespace KingsHandsProject.Services.Interfaces
{
    public interface IPokerHandParser
    {
        IReadOnlyList<PokerHand> ParseFile(string filePath);
    }
}