using UnityEngine;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Characters;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Integrates Character Editor with Jim's biographical aging system
    /// Transforms appearance authentically through life stages
    /// </summary>
    public class BiographicalCharacterEditor : MonoBehaviour, IAgeAwareCharacter
    {
        [Header("Biographical Character Settings")]
        [SerializeField] private BiographicalAgePreset[] agePresets;
        
        [Header("Character Editor Integration")]
        [SerializeField] private GameObject characterEditorPrefab;
        
        // Character Editor components (found at runtime)
        private Component characterBuilder;
        private Component equipmentManager;
        
        // Your existing systems
        private Age7Character age7Character;
        private AgeManager ageManager;
        
        [System.Serializable]
        public class BiographicalAgePreset
        {
            [Header("Life Stage Information")]
            public int age;
            public string lifeStageName;
            
            [Header("Physical Development")]
            public Vector3 bodyScale = Vector3.one;
            public Color skinTone = Color.white;
            public Color hairColor = new Color(0.4f, 0.2f, 0.1f);
            
            [Header("Age-Appropriate Styling")]
            public string[] clothingItems;
            public string[] accessories;
            public string hairStyle;
            public string facialHair;
            
            [Header("Biographical Context")]
            [TextArea(2, 4)]
            public string ageDescription;
        }
        
        private void Awake()
        {
            age7Character = GetComponent<Age7Character>();
            ageManager = FindFirstObjectByType<AgeManager>();
            
            // Find Character Editor components safely
            FindCharacterEditorComponents();
            
            // Create Jim's biographical age presets if not set
            if (agePresets == null || agePresets.Length == 0)
            {
                CreateJimsBiographicalPresets();
            }
        }
        
        private void FindCharacterEditorComponents()
        {
            // Use reflection to find Character Editor components without hard dependencies
            var builderType = System.Type.GetType("Assets.HeroEditor.Common.Scripts.CharacterScripts.CharacterBuilder");
            if (builderType != null)
            {
                characterBuilder = GetComponentInChildren(builderType);
            }
            
            var equipmentType = System.Type.GetType("Assets.HeroEditor.Common.Scripts.CharacterScripts.Equipment");
            if (equipmentType != null)
            {
                equipmentManager = GetComponentInChildren(equipmentType);
            }
            
            if (characterBuilder != null)
            {
                Debug.Log("[BiographicalCharacterEditor] Character Editor integration active for Jim's life journey");
            }
        }
        
        private void CreateJimsBiographicalPresets()
        {
            agePresets = new BiographicalAgePreset[]
            {
                new BiographicalAgePreset
                {
                    age = 7,
                    lifeStageName = "Innocent Childhood",
                    bodyScale = new Vector3(0.7f, 0.7f, 1f),
                    skinTone = new Color(1f, 0.9f, 0.8f),
                    hairColor = new Color(0.6f, 0.4f, 0.2f),
                    clothingItems = new[] { "SimpleShirt", "Shorts", "Sneakers" },
                    hairStyle = "ChildHair",
                    ageDescription = "Jim at 7 - curious, innocent, full of wonder about the world around him"
                },
                new BiographicalAgePreset
                {
                    age = 14,
                    lifeStageName = "Awkward Teenage Years",
                    bodyScale = new Vector3(0.85f, 0.85f, 1f),
                    skinTone = new Color(1f, 0.9f, 0.8f),
                    hairColor = new Color(0.5f, 0.3f, 0.15f),
                    clothingItems = new[] { "CasualShirt", "Jeans", "Sneakers" },
                    hairStyle = "TeenHair",
                    ageDescription = "Jim at 14 - navigating adolescence, trying to find his identity and place"
                },
                new BiographicalAgePreset
                {
                    age = 18,
                    lifeStageName = "Young Adulthood",
                    bodyScale = Vector3.one,
                    skinTone = new Color(1f, 0.9f, 0.8f),
                    hairColor = new Color(0.4f, 0.25f, 0.1f),
                    clothingItems = new[] { "CollegeShirt", "Jeans", "CasualShoes" },
                    hairStyle = "YoungAdultHair",
                    ageDescription = "Jim at 18 - confident young adult, ready to take on the world"
                },
                new BiographicalAgePreset
                {
                    age = 25,
                    lifeStageName = "Professional Establishment",
                    bodyScale = Vector3.one,
                    skinTone = new Color(0.95f, 0.85f, 0.75f),
                    hairColor = new Color(0.35f, 0.2f, 0.1f),
                    clothingItems = new[] { "BusinessShirt", "Slacks", "DressShoes" },
                    hairStyle = "ProfessionalHair",
                    facialHair = "LightBeard",
                    ageDescription = "Jim at 25 - building his career, developing mature perspectives on life"
                },
                new BiographicalAgePreset
                {
                    age = 50,
                    lifeStageName = "Distinguished Maturity",
                    bodyScale = new Vector3(1.05f, 1.02f, 1f),
                    skinTone = new Color(0.9f, 0.8f, 0.7f),
                    hairColor = new Color(0.6f, 0.6f, 0.6f),
                    clothingItems = new[] { "FormalShirt", "Slacks", "LeatherShoes" },
                    hairStyle = "MatureHair",
                    facialHair = "FullBeard",
                    ageDescription = "Jim at 50 - wise and distinguished, carrying decades of life experience"
                }
            };
            
            Debug.Log("[BiographicalCharacterEditor] Created biographical presets for Jim's authentic life journey");
        }
        
        #region IAgeAwareCharacter Implementation
        
        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            // Delegate to Age7Character for movement changes
            if (age7Character != null)
            {
                age7Character.ApplyAgeMovement(config);
            }
        }
        
        public void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;
            
            // Find matching biographical preset
            var preset = System.Array.Find(agePresets, p => p.age == profile.ageYears);
            if (preset != null)
            {
                ApplyBiographicalAppearance(preset);
            }
            
            // Delegate to Age7Character for other age changes
            if (age7Character != null)
            {
                age7Character.OnAgeChanged(profile);
            }
            
            Debug.Log($"[BiographicalCharacterEditor] Jim transformed to {preset?.lifeStageName} (Age {profile.ageYears})");
        }
        
        #endregion
        
        private void ApplyBiographicalAppearance(BiographicalAgePreset preset)
        {
            // Transform physical proportions authentically
            transform.localScale = preset.bodyScale;
            
            // Apply Character Editor styling if available
            if (characterBuilder != null)
            {
                ApplyCharacterEditorStyling(preset);
            }
            else
            {
                Debug.LogWarning("[BiographicalCharacterEditor] Character Editor not found - using scale-only aging");
            }
        }
        
        private void ApplyCharacterEditorStyling(BiographicalAgePreset preset)
        {
            try
            {
                var builderType = characterBuilder.GetType();
                
                var setSkinColorMethod = builderType.GetMethod("SetSkinColor");
                setSkinColorMethod?.Invoke(characterBuilder, new object[] { preset.skinTone });
                
                var setHairColorMethod = builderType.GetMethod("SetHairColor");
                setHairColorMethod?.Invoke(characterBuilder, new object[] { preset.hairColor });
                
                // Apply age-appropriate clothing and styling
                ApplyAgeAppropriateEquipment(preset);
                
                Debug.Log($"[BiographicalCharacterEditor] Applied authentic styling for {preset.lifeStageName}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BiographicalCharacterEditor] Character Editor styling failed: {e.Message}");
            }
        }
        
        private void ApplyAgeAppropriateEquipment(BiographicalAgePreset preset)
        {
            if (preset.clothingItems != null)
            {
                foreach (string item in preset.clothingItems)
                {
                    ApplyEquipmentItem(item);
                }
            }
        }
        
        private void ApplyEquipmentItem(string itemName)
        {
            try
            {
                if (equipmentManager != null)
                {
                    var equipMethod = equipmentManager.GetType().GetMethod("Equip");
                    equipMethod?.Invoke(equipmentManager, new object[] { itemName });
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BiographicalCharacterEditor] Could not equip {itemName}: {e.Message}");
            }
        }
        
        #region Development Tools
        
        [ContextMenu("Preview Jim's Life Journey")]
        private void PreviewLifeJourney()
        {
            if (agePresets == null) return;
            foreach (var preset in agePresets)
            {
                Debug.Log($"Age {preset.age}: {preset.lifeStageName} - {preset.ageDescription}");
            }
        }
        
        [ContextMenu("Test Random Age Transformation")]
        private void TestRandomAge()
        {
            if (agePresets == null || agePresets.Length == 0) return;
            var randomPreset = agePresets[Random.Range(0, agePresets.Length)];
            ApplyBiographicalAppearance(randomPreset);
            Debug.Log($"[Test] Applied {randomPreset.lifeStageName} appearance");
        }
        
        #endregion
    }
}

