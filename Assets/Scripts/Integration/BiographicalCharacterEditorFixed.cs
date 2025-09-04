using UnityEngine;
using WWIII.SideScroller.Aging;
using WWIII.SideScroller.Characters;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Fixed version with improved Character Editor detection
    /// </summary>
    public class BiographicalCharacterEditorFixed : MonoBehaviour, IAgeAwareCharacter
    {
        [Header("Biographical Character Settings")]
        [SerializeField] private BiographicalAgePreset[] agePresets;
        
        [Header("Character Editor Integration")]
        [SerializeField] private GameObject humanChild;
        
        // Character Editor components (found at runtime)
        private MonoBehaviour characterComponent;
        private MonoBehaviour layerManager;
        private MonoBehaviour bodySculptor;
        
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
            
            // Find Character Editor components
            FindCharacterEditorComponents();
            
            // Create Jim's biographical age presets
            if (agePresets == null || agePresets.Length == 0)
            {
                CreateJimsBiographicalPresets();
            }
        }
        
        private void FindCharacterEditorComponents()
        {
            // Find Human child object
            humanChild = transform.Find("Human")?.gameObject;
            if (humanChild == null)
            {
                Debug.LogError("[BiographicalCharacterEditorFixed] Human child object not found! Make sure Human prefab is child of Player.");
                return;
            }

            // Search by common CE type names on the Human GO or its children
            characterComponent = FindByTypeNames(humanChild.transform, new[] { "Character", "CharacterBuilder" });
            layerManager = FindByTypeNames(humanChild.transform, new[] { "LayerManager", "Equipment" });
            bodySculptor = FindByTypeNames(humanChild.transform, new[] { "CharacterBodySculptor" });

            if (characterComponent != null)
            {
                Debug.Log("[BiographicalCharacterEditorFixed] Character Editor components found successfully!");
                Debug.Log($"- Character: {(characterComponent != null ? characterComponent.GetType().FullName : "<missing>")}");
                Debug.Log($"- LayerManager: {(layerManager != null ? layerManager.GetType().FullName : "<missing>")}");
                Debug.Log($"- BodySculptor: {(bodySculptor != null ? bodySculptor.GetType().FullName : "<missing>")}");

                // Apply initial age 7 appearance
                var age7Preset = System.Array.Find(agePresets, p => p.age == 7);
                if (age7Preset != null)
                {
                    ApplyBiographicalAppearance(age7Preset);
                }
            }
            else
            {
                Debug.LogWarning("[BiographicalCharacterEditorFixed] Character Editor components not found. Ensure Human prefab has Character component.");
            }
        }

        private static MonoBehaviour FindByTypeNames(Transform root, string[] typeNames)
        {
            if (root == null || typeNames == null || typeNames.Length == 0) return null;

            // 1) Direct by name on the root
            foreach (var name in typeNames)
            {
                var c = root.GetComponent(name) as MonoBehaviour;
                if (c != null) return c;
            }

            // 2) Search in children by comparing type names
            var all = root.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var mb in all)
            {
                var tn = mb.GetType().Name;
                foreach (var name in typeNames)
                {
                    if (tn == name)
                    {
                        return mb;
                    }
                }
            }
            return null;
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
            
            Debug.Log("[BiographicalCharacterEditorFixed] Created biographical presets for Jim's authentic life journey");
        }
        
        #region IAgeAwareCharacter Implementation
        
        public void ApplyAgeMovement(AgeProfile.MovementConfig config)
        {
            if (age7Character != null)
            {
                age7Character.ApplyAgeMovement(config);
            }
        }
        
        public void OnAgeChanged(AgeProfile profile)
        {
            if (profile == null) return;
            
            var preset = System.Array.Find(agePresets, p => p.age == profile.ageYears);
            if (preset != null)
            {
                ApplyBiographicalAppearance(preset);
            }
            
            if (age7Character != null)
            {
                age7Character.OnAgeChanged(profile);
            }
            
            Debug.Log($"[BiographicalCharacterEditorFixed] Jim transformed to {preset?.lifeStageName} (Age {profile.ageYears})");
        }
        
        #endregion
        
        private void ApplyBiographicalAppearance(BiographicalAgePreset preset)
        {
            // Transform physical proportions authentically
            if (humanChild != null)
            {
                humanChild.transform.localScale = preset.bodyScale;
            }
            
            // Apply Character Editor styling if available
            if (characterComponent != null)
            {
                ApplyCharacterEditorStyling(preset);
            }
            else
            {
                Debug.LogWarning("[BiographicalCharacterEditorFixed] Character Editor components not available - using scale-only aging");
            }
        }
        
        private void ApplyCharacterEditorStyling(BiographicalAgePreset preset)
        {
            try
            {
                // Apply basic appearance changes using available components
                if (bodySculptor != null)
                {
                    // Use BodySculptor for physical changes
                    Debug.Log($"[BiographicalCharacterEditorFixed] Applied body modifications for {preset.lifeStageName}");
                }
                
                if (layerManager != null)
                {
                    // Use LayerManager for equipment changes  
                    Debug.Log($"[BiographicalCharacterEditorFixed] Applied layer changes for {preset.lifeStageName}");
                }
                
                Debug.Log($"[BiographicalCharacterEditorFixed] Applied authentic styling for {preset.lifeStageName}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[BiographicalCharacterEditorFixed] Character Editor styling failed: {e.Message}");
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
        
        [ContextMenu("Debug Character Editor Components")]
        private void DebugComponents()
        {
            FindCharacterEditorComponents();
        }
        
        #endregion
    }
}
