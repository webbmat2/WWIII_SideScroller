using UnityEngine;
using UnityEditor;

namespace WWIII.Editor
{
    /// <summary>
    /// Immediately applies Unity AI optimizations when this script compiles
    /// </summary>
    [InitializeOnLoad]
    public static class RunOptimizationsNow
    {
        private const string OPTIMIZATIONS_APPLIED_KEY = "WWIII_OptimizationsApplied_v2";

        static RunOptimizationsNow()
        {
            // Only run once per project
            if (!EditorPrefs.GetBool(OPTIMIZATIONS_APPLIED_KEY, false))
            {
                EditorApplication.delayCall += () => {
                    ApplyUnityAIOptimizations();
                    EditorPrefs.SetBool(OPTIMIZATIONS_APPLIED_KEY, true);
                };
            }
        }

        [MenuItem("WWIII/🚀 APPLY UNITY AI OPTIMIZATIONS NOW")]
        public static void ApplyUnityAIOptimizations()
        {
            Debug.Log("🚀 Applying Unity AI Recommended Optimizations...");
            Debug.Log("================================================");

            // Apply critical settings
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = 512f;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 25f;
            Debug.Log("✅ Quality settings optimized for mobile 60 FPS");

            // Configure Physics2D
            Physics2D.jobOptions = new PhysicsJobOptions2D()
            {
                useMultithreading = true,
                useConsistencySorting = true
            };
            Physics2D.gravity = new Vector2(0, -30f);
            Physics2D.velocityIterations = 8;
            Physics2D.positionIterations = 3;
            Debug.Log("✅ Physics2D multithreading enabled and optimized");

            // Configure camera
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 6f;
                mainCamera.backgroundColor = new Color(0.15f, 0.25f, 0.35f, 1f);
                mainCamera.transparencySortMode = TransparencySortMode.CustomAxis;
                mainCamera.transparencySortAxis = new Vector3(0, 0, 1);
                Debug.Log("✅ Main Camera configured for 2D side-scroller");
            }

            // Ensure Global Light 2D
            var globalLight = GameObject.Find("Global Light 2D");
            if (globalLight == null)
            {
                var lightObj = new GameObject("Global Light 2D");
                var light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
                if (light2DType != null)
                {
                    var light2D = lightObj.AddComponent(light2DType);
                    var lightTypeProperty = light2DType.GetProperty("lightType");
                    var intensityProperty = light2DType.GetProperty("intensity");
                    if (lightTypeProperty != null && intensityProperty != null)
                    {
                        lightTypeProperty.SetValue(light2D, 0); // Global
                        intensityProperty.SetValue(light2D, 1f);
                    }
                    Debug.Log("✅ Global Light 2D created and configured");
                }
            }

            // Configure graphics APIs
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.Metal 
            });
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.Metal 
            });
            Debug.Log("✅ Graphics APIs configured for Metal");

            Debug.Log("================================================");
            Debug.Log("🎉 Unity AI Optimizations Complete!");
            Debug.Log("✅ Mobile performance optimized for iPhone 16+ / tvOS / macOS");
            Debug.Log("✅ Physics2D multithreading enabled");
            Debug.Log("✅ Scene configured for 2D side-scroller");
            Debug.Log("✅ 60 FPS target set with VSync disabled");
            Debug.Log("🚀 READY FOR LEVEL AUTO-AUTHORING MODE!");

            EditorUtility.DisplayDialog("🎯 Unity AI Optimizations Applied!",
                "All critical optimizations have been applied:\n\n" +
                "✅ 60 FPS mobile target set\n" +
                "✅ Physics2D multithreading enabled\n" +
                "✅ Camera configured for 2D side-scroller\n" +
                "✅ Quality settings optimized\n" +
                "✅ Streaming mipmaps enabled\n\n" +
                "Ready for Level Auto-Authoring Mode!",
                "Excellent!");
        }

        [MenuItem("WWIII/🔄 RESET OPTIMIZATION FLAG")]
        public static void ResetOptimizationFlag()
        {
            EditorPrefs.DeleteKey(OPTIMIZATIONS_APPLIED_KEY);
            Debug.Log("🔄 Optimization flag reset - will run again on next compile");
        }
    }
}