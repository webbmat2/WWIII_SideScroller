using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace WWIII.Editor
{
    /// <summary>
    /// Validates project setup against Unity AI recommendations
    /// Shows current status and remaining issues
    /// </summary>
    public class ProjectStatusValidator : EditorWindow
    {
        [MenuItem("WWIII/📊 PROJECT STATUS VALIDATION")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectStatusValidator>("Project Status");
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("📊 Project Status Validation", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Unity AI Recommended Checklist", EditorStyles.helpBox);
            EditorGUILayout.Space();

            // URP Status
            CheckURPStatus();
            EditorGUILayout.Space();

            // Scene Status
            CheckSceneStatus();
            EditorGUILayout.Space();

            // Physics Status
            CheckPhysicsStatus();
            EditorGUILayout.Space();

            // Quality Status
            CheckQualityStatus();
            EditorGUILayout.Space();

            if (GUILayout.Button("🔄 REFRESH STATUS", GUILayout.Height(30)))
            {
                Repaint();
            }
        }

        private void CheckURPStatus()
        {
            EditorGUILayout.LabelField("🔥 URP 2D Configuration", EditorStyles.boldLabel);

            var urpAsset = GraphicsSettings.defaultRenderPipeline;
            if (urpAsset != null)
            {
                ShowStatus("✅", "URP Asset Found", Color.green);

                // Check HDR status (would need reflection to check private field)
                ShowStatus("💡", "HDR Status: Check manually in URP Asset", Color.yellow);

                // Check shadow distance
                ShowStatus("💡", "Shadow Distance: Check manually in URP Asset", Color.yellow);
            }
            else
            {
                ShowStatus("❌", "URP Asset Not Found", Color.red);
            }

            // Check camera transparency sort
            var camera = Camera.main;
            if (camera != null && camera.transparencySortMode == TransparencySortMode.CustomAxis)
            {
                ShowStatus("✅", "Camera Transparency Sort: Configured for 2D", Color.green);
            }
            else
            {
                ShowStatus("⚠️", "Camera Transparency Sort: Not optimized", Color.yellow);
            }
        }

        private void CheckSceneStatus()
        {
            EditorGUILayout.LabelField("🔥 Scene Configuration", EditorStyles.boldLabel);

            var camera = Camera.main;
            if (camera != null)
            {
                if (camera.orthographic)
                {
                    ShowStatus("✅", $"Main Camera: Orthographic, Size {camera.orthographicSize}", Color.green);
                }
                else
                {
                    ShowStatus("⚠️", "Main Camera: Not orthographic (should be for 2D)", Color.yellow);
                }

                var cameraDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
                if (cameraDataType != null)
                {
                    var cameraData = camera.GetComponent(cameraDataType);
                    if (cameraData != null)
                    {
                        ShowStatus("✅", "URP Camera Data: Present", Color.green);
                    }
                    else
                    {
                        ShowStatus("⚠️", "URP Camera Data: Missing", Color.yellow);
                    }
                }
                else
                {
                    ShowStatus("⚠️", "URP Camera Data: Type not found", Color.yellow);
                }
            }
            else
            {
                ShowStatus("❌", "Main Camera: Not found", Color.red);
            }

            var globalLight = GameObject.Find("Global Light 2D");
            if (globalLight != null)
            {
                ShowStatus("✅", "Global Light 2D: Present", Color.green);
            }
            else
            {
                ShowStatus("⚠️", "Global Light 2D: Missing", Color.yellow);
            }
        }

        private void CheckPhysicsStatus()
        {
            EditorGUILayout.LabelField("🔥 Physics2D Configuration", EditorStyles.boldLabel);

            if (Physics2D.jobOptions.useMultithreading)
            {
                ShowStatus("✅", "Physics2D Multithreading: Enabled", Color.green);
            }
            else
            {
                ShowStatus("⚠️", "Physics2D Multithreading: Disabled", Color.yellow);
            }

            ShowStatus("💡", $"Gravity: {Physics2D.gravity}", Color.white);
            ShowStatus("💡", $"Velocity Iterations: {Physics2D.velocityIterations}", Color.white);
            ShowStatus("💡", $"Position Iterations: {Physics2D.positionIterations}", Color.white);
        }

        private void CheckQualityStatus()
        {
            EditorGUILayout.LabelField("🔶 Quality Settings", EditorStyles.boldLabel);

            if (Application.targetFrameRate == 60)
            {
                ShowStatus("✅", "Target Frame Rate: 60 FPS", Color.green);
            }
            else
            {
                ShowStatus("⚠️", $"Target Frame Rate: {Application.targetFrameRate} (should be 60)", Color.yellow);
            }

            if (QualitySettings.vSyncCount == 0)
            {
                ShowStatus("✅", "VSync: Disabled (good for mobile)", Color.green);
            }
            else
            {
                ShowStatus("⚠️", "VSync: Enabled (may impact mobile performance)", Color.yellow);
            }

            if (QualitySettings.streamingMipmapsActive)
            {
                ShowStatus("✅", $"Streaming Mipmaps: Enabled ({QualitySettings.streamingMipmapsMemoryBudget}MB)", Color.green);
            }
            else
            {
                ShowStatus("⚠️", "Streaming Mipmaps: Disabled", Color.yellow);
            }

            ShowStatus("💡", $"Shadow Quality: {QualitySettings.shadows}", Color.white);
        }

        private void ShowStatus(string icon, string text, Color color)
        {
            var originalColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.LabelField($"{icon} {text}");
            GUI.color = originalColor;
        }
    }
}