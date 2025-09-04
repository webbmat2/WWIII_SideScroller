using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.Diagnostics
{
    public class EditorLogViewerWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _path;
        private string _content;
        private double _next;
        private const double Interval = 1.0; // seconds

        [MenuItem("WWIII/Diagnostics/Logs/Editor Log Viewer")] 
        public static void Open()
        {
            var win = GetWindow<EditorLogViewerWindow>(false, "Editor Log", true);
            win.minSize = new Vector2(600, 300);
            win.RefreshPath();
        }

        [MenuItem("WWIII/Diagnostics/Logs/Editor Log Viewer", true)]
        private static bool ValidateOpen()
        {
            return WWIII.SideScroller.Editor.Diagnostics.DiagnosticsMenu.IsEnabled;
        }

        private void RefreshPath()
        {
            try { _path = Application.consoleLogPath; }
            catch { _path = null; }
            if (string.IsNullOrEmpty(_path))
            {
#if UNITY_EDITOR_OSX
                _path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Logs/Unity/Editor/Editor.log";
#elif UNITY_EDITOR_WIN
                _path = Environment.GetEnvironmentVariable("LOCALAPPDATA") + @"\Unity\Editor\Editor.log";
#else
                _path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/.config/unity3d/Editor.log";
#endif
            }
        }

        private void OnGUI()
        {
            if (EditorApplication.timeSinceStartup > _next)
            {
                _next = EditorApplication.timeSinceStartup + Interval;
                LoadTail();
                Repaint();
            }
            EditorGUILayout.LabelField("Path:", _path ?? "(unknown)");
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_content ?? "(no log)");
            EditorGUILayout.EndScrollView();
        }

        private void LoadTail()
        {
            if (string.IsNullOrEmpty(_path) || !File.Exists(_path)) { _content = "(log not found)"; return; }
            try
            {
                // keep it simple: last ~10k chars
                var text = File.ReadAllText(_path);
                const int max = 10000;
                if (text.Length > max) text = text.Substring(text.Length - max);
                _content = text;
            }
            catch (Exception e)
            {
                _content = $"Error reading log: {e.Message}";
            }
        }
    }
}
