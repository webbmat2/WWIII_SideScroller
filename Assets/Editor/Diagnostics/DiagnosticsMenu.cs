using UnityEditor;

namespace WWIII.SideScroller.Editor.Diagnostics
{
    public static class DiagnosticsMenu
    {
        private const string Key = "WWIII_ShowDiagnostics";

        public static bool IsEnabled => EditorPrefs.GetBool(Key, true);

        [MenuItem("WWIII/Diagnostics/Toggle Dev Mode")] 
        public static void Toggle()
        {
            var v = EditorPrefs.GetBool(Key, true);
            EditorPrefs.SetBool(Key, !v);
        }
    }
}
