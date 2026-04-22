using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingsHandsProject.Infrastructure
{
    public static class DebugLogger
    {
        public static void Log(string message)
        {
            Debug.WriteLine($"[LOG] {message}");
        }

        public static void LogError(string message)
        {
            Debug.WriteLine($"[ERROR] {message}");
        }

        public static void LogException(System.Exception ex)
        {
            Debug.WriteLine($"[EXCEPTION] {ex}");
        }
    }
}