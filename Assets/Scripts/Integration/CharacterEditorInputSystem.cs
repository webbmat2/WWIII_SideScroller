using UnityEngine;
using UnityEngine.InputSystem;

namespace WWIII.SideScroller.Integration
{
    /// <summary>
    /// Replace Character Editor input with Input System integration.
    /// Wire Navigate/Select/Customize to your UI logic.
    /// </summary>
    public class CharacterEditorInputSystem : MonoBehaviour
    {
        [Header("Input Actions")]
        public InputActionAsset characterEditorInputActions;

        private InputAction navigateAction;
        private InputAction selectAction;
        private InputAction customizeAction;

        private void Awake()
        {
            if (characterEditorInputActions != null)
            {
                navigateAction = characterEditorInputActions.FindAction("Navigate");
                selectAction = characterEditorInputActions.FindAction("Select");
                customizeAction = characterEditorInputActions.FindAction("Customize");
            }
        }

        private void OnEnable()
        {
            characterEditorInputActions?.Enable();
        }

        private void OnDisable()
        {
            characterEditorInputActions?.Disable();
        }

        private void Update()
        {
            HandleCharacterEditorInput();
        }

        private void HandleCharacterEditorInput()
        {
            if (selectAction != null && selectAction.WasPressedThisFrame())
            {
                HandleCharacterSelection();
            }

            if (customizeAction != null && customizeAction.WasPressedThisFrame())
            {
                HandleCharacterCustomization();
            }
        }

        private void HandleCharacterSelection()
        {
            Debug.Log("[CharacterEditorInputSystem] Character selection triggered");
        }

        private void HandleCharacterCustomization()
        {
            Debug.Log("[CharacterEditorInputSystem] Character customization triggered");
        }
    }
}

