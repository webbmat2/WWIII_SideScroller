using UnityEngine;

namespace WWIII.Core
{
    public class MobileOptimizer : MonoBehaviour
    {
        [Header("iOS Settings")]
        [SerializeField] private bool enableHaptics = true; // iOS haptic feedback
        [SerializeField] private bool enableProMotionSupport = true; // 120Hz displays
        [SerializeField] private bool optimizeForBattery = true; // Battery optimizations
        
        [Header("Performance Settings")]
        [SerializeField] private bool enableGPUInstancing = true; // GPU instancing
        [SerializeField] private bool enableDynamicBatching = true; // Dynamic batching
        [SerializeField] private int maxTextureSize = 2048; // Max texture size limit
        
        [Header("Audio Settings")]
        [SerializeField] private bool muteOnBackground = false;
        [SerializeField] private float masterVolume = 1f;
        
        public static MobileOptimizer Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeMobileOptimizations();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeMobileOptimizations()
        {
            SetupGraphicsOptimizations();
            SetupAudioOptimizations();
            SetupIOSSpecificSettings();
            
            Debug.Log($"Mobile optimizations applied for WWIII SideScroller - ProMotion: {enableProMotionSupport}, Battery: {optimizeForBattery}");
        }
        
        private void SetupGraphicsOptimizations()
        {
            // Always check the settings to avoid CS0414 warnings
            bool useGPUInstancing = enableGPUInstancing;
            bool useDynamicBatching = enableDynamicBatching;
            
            #if UNITY_ANDROID || UNITY_IOS

            // Note: PlayerSettings is editor-only; avoid using it at runtime.
            // Dynamic batching, instancing and skin weights are configured via Quality Settings and renderer settings.
            QualitySettings.masterTextureLimit = GetTextureLimitBasedOnDevice();
            QualitySettings.lodBias = 1f;
            
            #else
            
            // Ensure fields are used in Editor to avoid warnings
            Debug.Log($"Graphics optimization settings: GPU Instancing={useGPUInstancing}, Dynamic Batching={useDynamicBatching}");
            
            #endif
        }
        
        private int GetTextureLimitBasedOnDevice()
        {
            // Always use maxTextureSize to avoid CS0414 warning
            int configuredMaxSize = maxTextureSize;
            
            #if UNITY_IOS || UNITY_ANDROID
            
            int memorySize = SystemInfo.systemMemorySize;
            int calculatedLimit = 0;
            
            if (memorySize >= 8192) // 8GB+ devices
            {
                calculatedLimit = 0; // Full resolution
            }
            else if (memorySize >= 4096) // 4-8GB devices  
            {
                calculatedLimit = Mathf.Max(0, QualitySettings.masterTextureLimit);
            }
            else // <4GB devices
            {
                calculatedLimit = 1; // Half resolution textures
            }
            
            // Ensure we don't exceed the configured max texture size limit
            if (configuredMaxSize > 0)
            {
                return Mathf.Max(calculatedLimit, GetTextureLimitFromSize(configuredMaxSize));
            }
            
            return calculatedLimit;
            
            #else
            
            // Use configured max size in Editor to avoid warnings
            return GetTextureLimitFromSize(configuredMaxSize);
            
            #endif
        }
        
        private int GetTextureLimitFromSize(int maxSize)
        {
            // Convert max texture size to Unity's texture limit scale
            return maxSize switch
            {
                >= 2048 => 0,  // Full resolution
                >= 1024 => 1,  // Half resolution  
                >= 512 => 2,   // Quarter resolution
                _ => 3         // Eighth resolution
            };
        }
        
        private void SetupAudioOptimizations()
        {
            AudioListener.volume = masterVolume;
            
            #if UNITY_IOS
            if (muteOnBackground)
            {
                AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            }
            #endif
        }
        
        private void SetupIOSSpecificSettings()
        {
            // Always check the settings to avoid CS0414 warnings
            bool useProMotion = enableProMotionSupport;
            bool batteryOptimized = optimizeForBattery;
            
            #if UNITY_IOS
            
            if (useProMotion)
            {
                Application.targetFrameRate = 120;
            }
            else
            {
                Application.targetFrameRate = 60;
            }
            
            if (batteryOptimized)
            {
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = 60; // Override ProMotion for battery
            }
            
            #else
            
            // Ensure fields are used in Editor to avoid warnings
            Debug.Log($"iOS optimization settings: ProMotion={useProMotion}, Battery Optimized={batteryOptimized}");
            
            #endif
        }
        
        private void OnAudioConfigurationChanged(bool deviceWasChanged)
        {
            #if UNITY_IOS
            if (deviceWasChanged)
            {
                Debug.Log("Audio configuration changed on iOS device");
            }
            #endif
        }
        
        public void TriggerHapticFeedback(HapticFeedbackType feedbackType = HapticFeedbackType.LightImpact)
        {
            if (!enableHaptics) return;
            
            #if UNITY_IOS && !UNITY_EDITOR
            switch (feedbackType)
            {
                case HapticFeedbackType.LightImpact:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactLight);
                    break;
                case HapticFeedbackType.MediumImpact:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactMedium);
                    break;
                case HapticFeedbackType.HeavyImpact:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.ImpactHeavy);
                    break;
                case HapticFeedbackType.Success:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.NotificationSuccess);
                    break;
                case HapticFeedbackType.Warning:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.NotificationWarning);
                    break;
                case HapticFeedbackType.Error:
                    iOSHapticFeedback.Instance.Trigger(iOSHapticFeedback.iOSFeedbackType.NotificationError);
                    break;
            }
            #elif UNITY_ANDROID && !UNITY_EDITOR
            // Use basic vibration on Android
            switch (feedbackType)
            {
                case HapticFeedbackType.LightImpact:
                case HapticFeedbackType.Success:
                    Handheld.Vibrate();
                    break;
                case HapticFeedbackType.MediumImpact:
                case HapticFeedbackType.Warning:
                    Handheld.Vibrate();
                    break;
                case HapticFeedbackType.HeavyImpact:
                case HapticFeedbackType.Error:
                    Handheld.Vibrate();
                    break;
            }
            #endif
        }
        
        public void SetHapticEnabled(bool enabled)
        {
            enableHaptics = enabled;
        }
        
        public bool IsHapticEnabled => enableHaptics;
        
        public void OptimizeForDevice()
        {
            #if UNITY_ANDROID
            OptimizeForAndroid();
            #elif UNITY_IOS
            OptimizeForIOS();
            #endif
        }
        
        private void OptimizeForAndroid()
        {
            QualitySettings.shadowDistance = 15f;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.antiAliasing = 0;
        }
        
        private void OptimizeForIOS()
        {
            QualitySettings.shadowDistance = 20f;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.antiAliasing = 2;
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && muteOnBackground)
            {
                AudioListener.volume = 0f;
            }
            else
            {
                AudioListener.volume = masterVolume;
            }
        }
    }
    
    public enum HapticFeedbackType
    {
        LightImpact,
        MediumImpact,
        HeavyImpact,
        Success,
        Warning,
        Error
    }
    
    #if UNITY_IOS && !UNITY_EDITOR
    public class iOSHapticFeedback
    {
        public static iOSHapticFeedback Instance = new iOSHapticFeedback();
        
        public enum iOSFeedbackType
        {
            ImpactLight,
            ImpactMedium,
            ImpactHeavy,
            NotificationSuccess,
            NotificationWarning,
            NotificationError,
            Selection
        }
        
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _triggerHapticFeedback(int type);
        
        public void Trigger(iOSFeedbackType type)
        {
            _triggerHapticFeedback((int)type);
        }
    }
    #endif
}
