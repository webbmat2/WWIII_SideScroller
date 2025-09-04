using UnityEngine;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Bridges your existing AgeManager with Character Editor for seamless biographical progression
    /// </summary>
    public class AgeManagerCharacterEditorBridge : MonoBehaviour
    {
        [Header("Integration Settings")]
        [SerializeField] private bool enableCharacterEditorIntegration = true;
        [SerializeField] private bool logAgeTransitions = true;
        
        private AgeManager ageManager;
        private IAgeAwareCharacter appearanceController;
        
        private void Awake()
        {
            ageManager = GetComponent<AgeManager>();
            // Prefer Spine controller, then Sprite Library
            var spineController = FindFirstObjectByType<WWIII.SideScroller.Characters.SpineAppearanceController>();
            appearanceController = spineController as IAgeAwareCharacter;
            if (appearanceController == null)
            {
                var spriteLibController = FindFirstObjectByType<WWIII.SideScroller.Characters.CharacterAppearanceController>();
                appearanceController = spriteLibController as IAgeAwareCharacter;
            }
            if (appearanceController == null)
            {
                // Fallbacks: new CE fixed, then legacy CE
                var fixedEditor = FindFirstObjectByType<BiographicalCharacterEditorFixed>();
                appearanceController = fixedEditor as IAgeAwareCharacter;
                if (appearanceController == null)
                {
                    var legacy = FindFirstObjectByType<BiographicalCharacterEditor>();
                    appearanceController = legacy as IAgeAwareCharacter;
                }
            }
            
            if (ageManager == null)
            {
                Debug.LogError("[AgeManagerCharacterEditorBridge] AgeManager not found!");
                return;
            }
            
            if (appearanceController == null && enableCharacterEditorIntegration)
            {
                Debug.LogWarning("[AgeManagerCharacterEditorBridge] Appearance controller not found - Sprite Library integration disabled");
                enableCharacterEditorIntegration = false;
            }
        }
        
        private void Start()
        {
            if (enableCharacterEditorIntegration && ageManager != null && appearanceController != null)
            {
                ageManager.OnAgeChanged += HandleAgeChangedForCharacterEditor;
                Debug.Log("[AgeManagerCharacterEditorBridge] Character Editor integration active");
            }
        }
        
        private void OnDestroy()
        {
            if (ageManager != null)
            {
                ageManager.OnAgeChanged -= HandleAgeChangedForCharacterEditor;
            }
        }
        
        private void HandleAgeChangedForCharacterEditor(AgeProfile profile)
        {
            if (!enableCharacterEditorIntegration || appearanceController == null) return;
            
            if (logAgeTransitions)
            {
                Debug.Log($"[AgeManagerCharacterEditorBridge] Age transition: {profile.ageYears} years - updating Character Editor appearance");
            }
            
            appearanceController.OnAgeChanged(profile);
            
            TriggerBiographicalEffects(profile);
        }
        
        private void TriggerBiographicalEffects(AgeProfile profile)
        {
            switch (profile.ageYears)
            {
                case 7:
                    Debug.Log("[BiographicalEffects] Childhood innocence - bright, curious animations");
                    break;
                case 14:
                    Debug.Log("[BiographicalEffects] Teenage awkwardness - uncertain movements");
                    break;
                case 18:
                    Debug.Log("[BiographicalEffects] Young adult confidence - strong, purposeful actions");
                    break;
                case 25:
                    Debug.Log("[BiographicalEffects] Professional maturity - measured, deliberate movements");
                    break;
                case 50:
                    Debug.Log("[BiographicalEffects] Wise maturity - slower but more thoughtful actions");
                    break;
            }
        }
        
        #region Public Methods for External Integration
        
        public async void TriggerAgeTransition(int targetAge)
        {
            if (ageManager != null)
            {
                try
                {
                    await ageManager.RequestAgeIndexAsync(targetAge, true);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[AgeManagerCharacterEditorBridge] Age transition failed: {e.Message}");
                }
            }
        }
        
        public void EnableCharacterEditorIntegration(bool enable)
        {
            enableCharacterEditorIntegration = enable;
            Debug.Log($"[AgeManagerCharacterEditorBridge] Character Editor integration: {(enable ? "Enabled" : "Disabled")}");
        }
        
        #endregion
        
        #region Development Tools
        
        [ContextMenu("Test Age Progression Sequence")]
        private void TestAgeProgressionSequence()
        {
            if (ageManager == null) return;
            StartCoroutine(TestAgeSequence());
        }
        
        private System.Collections.IEnumerator TestAgeSequence()
        {
            int[] ageSequence = { 7, 14, 18, 25, 50 };
            
            foreach (int age in ageSequence)
            {
                TriggerAgeTransition(age);
                yield return new UnityEngine.WaitForSeconds(3f);
            }
            
            Debug.Log("[AgeManagerCharacterEditorBridge] Age progression sequence complete");
        }
        
        #endregion
    }
}
