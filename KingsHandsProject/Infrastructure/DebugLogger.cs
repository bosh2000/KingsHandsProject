using System.Diagnostics;

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