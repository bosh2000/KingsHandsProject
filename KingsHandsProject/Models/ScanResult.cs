using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingsHandsProject.Models
{
    public sealed class ScanResult
    {
        public int ProcessedFiles { get; set; }
        public int TotalFiles { get; set; }
        public string? CurrentFile { get; set; }
    }
}