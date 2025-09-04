using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Diagnostics
{
    [InitializeOnLoad]
    public static class ErrorEventForwarder
    {
        private static readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
        private static string _filePath;
        private static double _next;

        static ErrorEventForwarder()
        {
            try
            {
                var logsDir = Path.Combine(Application.dataPath, "../Logs");
                Directory.CreateDirectory(logsDir);
                _filePath = Path.GetFullPath(Path.Combine(logsDir, "unity_error_events.jsonl"));
            }
            catch { _filePath = null; }
            Application.logMessageReceivedThreaded += OnLog;
            EditorApplication.update += Flush;
        }

        private static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(_filePath)) return;
            try
            {
                var json = $"{{\"ts\":\"{DateTime.UtcNow:o}\",\"type\":\"{type}\",\"msg\":{Escape(condition)},\"stack\":{Escape(stackTrace)} }}";
                _queue.Enqueue(json);
            }
            catch { }
        }

        private static string Escape(string s)
        {
            if (s == null) return "\"\"";
            return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n","\\n").Replace("\r","\\r") + "\"";
        }

        private static void Flush()
        {
            if (EditorApplication.timeSinceStartup < _next) return;
            _next = EditorApplication.timeSinceStartup + 0.5f;
            if (string.IsNullOrEmpty(_filePath)) return;
            if (_queue.IsEmpty) return;
            try
            {
                using (var fs = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    while (_queue.TryDequeue(out var line)) sw.WriteLine(line);
                }
            }
            catch { }
        }

        [MenuItem("WWIII/Diagnostics/Logs/Open Error Events (JSONL)")]
        public static void OpenFile()
        {
            if (string.IsNullOrEmpty(_filePath)) return;
            EditorUtility.RevealInFinder(_filePath);
        }

        [MenuItem("WWIII/Diagnostics/Logs/Open Error Events (JSONL)", true)]
        private static bool ValidateOpenFile()
        {
            return WWIII.SideScroller.Editor.Diagnostics.DiagnosticsMenu.IsEnabled;
        }
    }
}
