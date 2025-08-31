using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace WWIII.Editor
{
    /// <summary>
    /// Auto-runs Unity AI cleanup when this script loads
    /// Implements all critical optimizations immediately
    /// </summary>
    [InitializeOnLoad]
    public static class AutoCleanupRunner
    {
        private const string CLEANUP_APPLIED_KEY = "WWIII_CleanupApplied";

        static AutoCleanupRunner()
        {
            // Only run once per project
            if (!EditorPrefs.GetBool(CLEANUP_APPLIED_KEY, false))
            {
                EditorApplication.delayCall += RunAutomaticCleanup;
            }
        }

        [MenuItem("WWIII/🚀 RUN UNITY AI CLEANUP NOW")]
        public static void RunCleanupManually()
        {
            RunAutomaticCleanup();
        }

        private static void RunAutomaticCleanup()
        {
            Debug.Log("🚀 WWIII SideScroller - Unity AI Automatic Cleanup Starting...");
            Debug.Log("================================================");

            // 1. Configure URP for Mobile Performance
            ConfigureURPForMobile();

            // 2. Setup Scene Foundation  
            SetupSceneFoundation();

            // 3. Optimize Physics2D
            OptimizePhysics2D();

            // 4. Configure Player Settings
            ConfigurePlayerSettings();

            // 5. Mark as complete
            EditorPrefs.SetBool(CLEANUP_APPLIED_KEY, true);

            Debug.Log("================================================");
            Debug.Log("🎉 Unity AI Cleanup Complete! Project Ready for Level Auto-Authoring!");

            // Show completion dialog
            if (EditorUtility.DisplayDialog("🎯 Project Optimized!",
                "Unity AI cleanup complete!\n\n" +
                "✅ URP 2D optimized for mobile\n" +
                "✅ Physics2D multithreading enabled\n" +
                "✅ Scene configured for level creation\n" +
                "✅ 60 FPS mobile target set\n\n" +
                "Ready to create Level 1 Tutorial?",
                "Start Level Creation", "OK"))
            {
                // Open Level1Setup tool
                EditorApplication.ExecuteMenuItem("WWIII/🎮 CREATE LEVEL 1 SCENE");
            }
        }

        private static void ConfigureURPForMobile()
        {
            Debug.Log("🔥 Configuring URP 2D for Mobile Performance...");

            // Set render scale for mobile performance
            var urpAsset = GraphicsSettings.defaultRenderPipeline;
            if (urpAsset != null)
            {
                // Use reflection to access private fields
                var urpAssetType = urpAsset.GetType();

                // Disable HDR for mobile
                SetPrivateField(urpAsset, urpAssetType, "m_SupportsHDR", false);

                // Reduce shadow distance
                SetPrivateField(urpAsset, urpAssetType, "m_ShadowDistance", 25f);

                // Set render scale for mobile optimization
                SetPrivateField(urpAsset, urpAssetType, "m_RenderScale", 0.9f);

                EditorUtility.SetDirty(urpAsset);
                Debug.Log("✅ URP configured: HDR disabled, shadows optimized, render scale 0.9");
            }

            // Configure transparency sorting for 2D
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transparencySortMode = TransparencySortMode.CustomAxis;
                camera.transparencySortAxis = new Vector3(0, 0, 1);
                Debug.Log("✅ Camera transparency sort configured for 2D");
            }
        }

        private static void SetupSceneFoundation()
        {
            Debug.Log("🔥 Setting Up Scene Foundation...");

            var camera = Camera.main;
            if (camera != null)
            {
                // Configure for 2D side-scroller
                camera.orthographic = true;
                camera.orthographicSize = 6f;
                camera.backgroundColor = new Color(0.15f, 0.25f, 0.35f, 1f); // Dark theme for WWIII

                // Ensure URP Camera Data
                var cameraDataType = System.Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
                if (cameraDataType != null)
                {
                    var cameraData = camera.GetComponent(cameraDataType);
                    if (cameraData == null)
                    {
                        cameraData = camera.gameObject.AddComponent(cameraDataType);
                    }
                }

                Debug.Log("✅ Main Camera configured for 2D side-scroller");
            }

            // Ensure Global Light 2D exists and is configured
            var globalLight = GameObject.Find("Global Light 2D");
            if (globalLight == null)
            {
                var lightObj = new GameObject("Global Light 2D");
                var light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
                if (light2DType != null)
                {
                    lightObj.AddComponent(light2DType);
                    Debug.Log("✅ Created Global Light 2D");
                }
            }
        }

        private static void OptimizePhysics2D()
        {
            Debug.Log("🔥 Optimizing Physics2D for Multi-Core Performance...");

            // Enable multithreading for iPhone 16+ / tvOS multi-core performance
            Physics2D.jobOptions = new PhysicsJobOptions2D()
            {
                useMultithreading = true,
                useConsistencySorting = true
            };

            // Optimize gravity for side-scroller
            Physics2D.gravity = new Vector2(0, -30f);

            // Optimize solver iterations for performance/quality balance
            Physics2D.velocityIterations = 8;
            Physics2D.positionIterations = 3;

            Debug.Log("✅ Physics2D multithreading enabled, gravity optimized");
        }

        private static void ConfigurePlayerSettings()
        {
            Debug.Log("🔥 Configuring Player Settings for Mobile...");

            // Set target frame rate for 60 FPS mobile
            Application.targetFrameRate = 60;

            // Quality settings for mobile optimization
            QualitySettings.vSyncCount = 0; // Disable VSync for mobile battery
            QualitySettings.shadows = ShadowQuality.Disable; // Disable shadows for mobile
            QualitySettings.streamingMipmapsActive = true; // Enable streaming mipmaps
            QualitySettings.streamingMipmapsMemoryBudget = 512f; // 512MB budget

            Debug.Log("✅ Player settings optimized: 60 FPS target, VSync disabled, streaming mipmaps enabled");
        }

        private static void SetPrivateField(object obj, System.Type type, string fieldName, object value)
        {
            var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }

        [MenuItem("WWIII/🔄 RESET CLEANUP FLAG")]
        public static void ResetCleanupFlag()
        {
            EditorPrefs.DeleteKey(CLEANUP_APPLIED_KEY);
            Debug.Log("🔄 Cleanup flag reset - will run again on next compile");
        }
    }
}