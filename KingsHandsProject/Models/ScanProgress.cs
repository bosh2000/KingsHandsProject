using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingsHandsProject.Models
{
    public sealed class ScanProgress
    {
        public List<PokerHand> Hands { get; } = new();
        public int ProcessedFilesCount { get; set; }
        public int SkippedFilesCount { get; set; }
        public List<string> Errors { get; } = new();
    }
}