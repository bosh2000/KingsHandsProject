using KingsHandsProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingsHandsProject.Services.Interfaces
{
    public interface IPokerHandParser
    {
        IReadOnlyList<PokerHand> ParseFile(string filePath);
    }
}