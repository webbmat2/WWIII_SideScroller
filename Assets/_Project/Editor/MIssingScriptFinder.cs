// Assets/_Project/Editor/MIssingScriptFinder.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public sealed class MissingScriptsFinder : EditorWindow
{
    private readonly List<GameObject> _withMissing = new List<GameObject>();
    private int _index = -1;

    [MenuItem("Tools/Diagnostics/Missing Scripts Finder")]
    private static void Open() => GetWindow<MissingScriptsFinder>("Missing Scripts");

    private void OnGUI()
    {
        if (GUILayout.Button("Scan Scene"))
            Scan();

        EditorGUILayout.LabelField($"Found: {_withMissing.Count} objects with missing scripts.");
        using (new EditorGUI.DisabledScope(_withMissing.Count == 0))
        {
            if (GUILayout.Button("Select Next"))
            {
                _index = (_index + 1) % _withMissing.Count;
                var go = _withMissing[_index];
                Selection.activeObject = go;
                EditorGUIUtility.PingObject(go);
            }
        }

        EditorGUILayout.HelpBox("Use Scan, then Select Next to locate and remove missing script components.", MessageType.Info);
    }

    private void Scan()
    {
        _withMissing.Clear();
        _index = -1;

        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
        {
            // skip assets and hidden hierarchy objects
            if (EditorUtility.IsPersistent(go)) continue;
            if ((go.hideFlags & HideFlags.HideInHierarchy) != 0) continue;

            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    _withMissing.Add(go);
                    break;
                }
            }
        }

        if (_withMissing.Count > 0) _index = 0;
    }
}
#endif