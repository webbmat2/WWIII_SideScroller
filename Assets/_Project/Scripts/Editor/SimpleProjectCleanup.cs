using UnityEngine;
using UnityEditor;

namespace WWIII.Editor
{
    /// <summary>
    /// Simplified Unity AI cleanup tool that applies critical optimizations
    /// without complex URP type dependencies
    /// </summary>
    public class SimpleProjectCleanup : EditorWindow
    {
        [MenuItem("WWIII/🚀 APPLY UNITY AI OPTIMIZATIONS")]
        public static void ApplyOptimizations()
        {
            Debug.Log("🚀 Applying Unity AI Recommended Optimizations...");
            Debug.Log("================================================");

            ApplyCriticalSettings();
            ConfigureSceneBasics();
            OptimizePhysics();

            Debug.Log("================================================");
            Debug.Log("🎉 Unity AI Optimizations Complete!");

            EditorUtility.DisplayDialog("✅ Optimizations Applied!",
                "Critical Unity AI recommendations have been applied:\n\n" +
                "• Physics2D multithreading enabled\n" +
                "• 60 FPS target set\n" +
                "• Quality settings optimized\n" +
                "• Scene camera configured\n\n" +
                "Ready for Level Auto-Authoring Mode!",
                "Excellent!");
        }

        private static void ApplyCriticalSettings()
        {
            Debug.Log("🔥 Applying Critical Performance Settings...");

            // Set 60 FPS target for mobile
            Application.targetFrameRate = 60;
            Debug.Log("✅ Target frame rate set to 60 FPS");

            // Disable VSync for mobile battery optimization
            QualitySettings.vSyncCount = 0;
            Debug.Log("✅ VSync disabled for mobile battery optimization");

            // Enable streaming mipmaps for texture memory optimization
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = 512f; // 512MB for mobile
            Debug.Log("✅ Streaming mipmaps enabled with 512MB budget");

            // Optimize shadow settings for mobile
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 25f;
            Debug.Log("✅ Shadows optimized for mobile performance");

            // Configure graphics APIs for target platforms
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.Metal 
            });
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.Metal 
            });
            Debug.Log("✅ Graphics APIs configured for Metal (iOS/macOS)");
        }

        private static void ConfigureSceneBasics()
        {
            Debug.Log("🔥 Configuring Scene for 2D Side-Scroller...");

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // Configure for 2D orthographic
                mainCamera.orthographic = true;
                mainCamera.orthographicSize = 6f;
                mainCamera.backgroundColor = new Color(0.15f, 0.25f, 0.35f, 1f); // WWIII dark theme

                // Set transparency sorting for 2D
                mainCamera.transparencySortMode = TransparencySortMode.CustomAxis;
                mainCamera.transparencySortAxis = new Vector3(0, 0, 1);

                Debug.Log("✅ Main Camera configured for 2D side-scroller");
            }

            // Ensure Global Light 2D exists
            var globalLight = GameObject.Find("Global Light 2D");
            if (globalLight == null)
            {
                var lightObj = new GameObject("Global Light 2D");
                
                // Try to add Light2D component via reflection to avoid direct URP dependency
                var light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
                if (light2DType != null)
                {
                    var light2D = lightObj.AddComponent(light2DType);
                    
                    // Set basic properties via reflection
                    var lightTypeProperty = light2DType.GetProperty("lightType");
                    var intensityProperty = light2DType.GetProperty("intensity");
                    
                    if (lightTypeProperty != null && intensityProperty != null)
                    {
                        // LightType.Global = 0
                        lightTypeProperty.SetValue(light2D, 0);
                        intensityProperty.SetValue(light2D, 1f);
                    }
                    
                    Debug.Log("✅ Global Light 2D created and configured");
                }
                else
                {
                    Debug.LogWarning("⚠️ Could not find Light2D type - URP may need manual configuration");
                }
            }
        }

        private static void OptimizePhysics()
        {
            Debug.Log("🔥 Optimizing Physics2D for Multi-Core Performance...");

            // Enable Physics2D multithreading for iPhone 16+/tvOS performance
            Physics2D.jobOptions = new PhysicsJobOptions2D()
            {
                useMultithreading = true,
                useConsistencySorting = true
            };
            Debug.Log("✅ Physics2D multithreading enabled");

            // Set gravity optimized for side-scroller feel
            Physics2D.gravity = new Vector2(0, -30f);
            Debug.Log("✅ Gravity set to -30 for side-scroller");

            // Optimize solver iterations for performance/quality balance
            Physics2D.velocityIterations = 8;
            Physics2D.positionIterations = 3;
            Debug.Log("✅ Physics solver iterations optimized");
        }

        [MenuItem("WWIII/📊 CHECK OPTIMIZATION STATUS")]
        public static void CheckOptimizationStatus()
        {
            Debug.Log("📊 Current Project Optimization Status:");
            Debug.Log("=====================================");

            // Check frame rate
            Debug.Log($"Target Frame Rate: {Application.targetFrameRate} {(Application.targetFrameRate == 60 ? "✅" : "⚠️")}");

            // Check VSync
            Debug.Log($"VSync Count: {QualitySettings.vSyncCount} {(QualitySettings.vSyncCount == 0 ? "✅" : "⚠️")}");

            // Check streaming mipmaps
            Debug.Log($"Streaming Mipmaps: {QualitySettings.streamingMipmapsActive} {(QualitySettings.streamingMipmapsActive ? "✅" : "⚠️")}");

            // Check physics
            Debug.Log($"Physics Multithreading: {Physics2D.jobOptions.useMultithreading} {(Physics2D.jobOptions.useMultithreading ? "✅" : "⚠️")}");

            // Check camera
            var camera = Camera.main;
            if (camera != null)
            {
                Debug.Log($"Camera Orthographic: {camera.orthographic} {(camera.orthographic ? "✅" : "⚠️")}");
                Debug.Log($"Camera Size: {camera.orthographicSize}");
            }

            Debug.Log("=====================================");
        }
    }
}