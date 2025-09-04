using UnityEngine;

namespace WWIII.SideScroller.Utils
{
    public class TextureStreamingConfigurator : MonoBehaviour
    {
        [Tooltip("Enable streaming mipmaps for textures.")]
        public bool enableStreaming = true;
        public int budgetMB = 128;

        private void Awake()
        {
            QualitySettings.streamingMipmapsActive = enableStreaming;
            QualitySettings.streamingMipmapsRenderersPerFrame = 64;
            QualitySettings.streamingMipmapsMaxLevelReduction = 2;
            QualitySettings.streamingMipmapsMemoryBudget = budgetMB;
        }
    }
}

