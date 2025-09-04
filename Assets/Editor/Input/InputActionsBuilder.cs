using UnityEditor;
#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
#endif

namespace WWIII.SideScroller.Editor.Input
{
    public static class InputActionsBuilder
    {
        [MenuItem("WWIII/Input/Create Default Action Maps")] 
        public static void CreateDefault()
        {
#if ENABLE_INPUT_SYSTEM
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();

            // Player map
            var player = new InputActionMap("Player");
            player.AddAction("Move", InputActionType.Value, null, "Vector2");
            player.AddAction("Jump", InputActionType.Button);
            player.AddAction("Dash", InputActionType.Button);
            player.AddAction("Interact", InputActionType.Button);
            player.AddAction("Shoot", InputActionType.Button);
            player.AddAction("Album", InputActionType.Button);
            player.AddAction("Pause", InputActionType.Button);

            // Bindings – Gamepad + Keyboard
            player["Move"].AddCompositeBinding("2DVector")
                .With("Up", "<Gamepad>/leftStick/up")
                .With("Down", "<Gamepad>/leftStick/down")
                .With("Left", "<Gamepad>/leftStick/left")
                .With("Right", "<Gamepad>/leftStick/right");
            player["Move"].AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            player["Jump"].AddBinding("<Gamepad>/buttonSouth").WithGroup("Gamepad");
            player["Jump"].AddBinding("<Keyboard>/space");
            player["Dash"].AddBinding("<Gamepad>/buttonEast");
            player["Dash"].AddBinding("<Keyboard>/leftShift");
            player["Interact"].AddBinding("<Gamepad>/buttonWest");
            player["Interact"].AddBinding("<Keyboard>/e");
            player["Shoot"].AddBinding("<Gamepad>/rightTrigger");
            player["Shoot"].AddBinding("<Keyboard>/leftCtrl");
            player["Album"].AddBinding("<Gamepad>/leftShoulder");
            player["Album"].AddBinding("<Keyboard>/tab");
            player["Pause"].AddBinding("<Gamepad>/start");
            player["Pause"].AddBinding("<Keyboard>/escape");

            // Child map (simplified)
            var child = new InputActionMap("Child");
            child.AddAction("Move", InputActionType.Value, null, "Vector2");
            child.AddAction("Jump", InputActionType.Button);
            child.AddAction("Interact", InputActionType.Button);
            child["Move"].AddCompositeBinding("2DVector")
                .With("Up", "<Gamepad>/leftStick/up")
                .With("Down", "<Gamepad>/leftStick/down")
                .With("Left", "<Gamepad>/leftStick/left")
                .With("Right", "<Gamepad>/leftStick/right");
            child["Move"].AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            child["Jump"].AddBinding("<Gamepad>/buttonSouth");
            child["Jump"].AddBinding("<Keyboard>/space");
            child["Interact"].AddBinding("<Gamepad>/buttonWest");
            child["Interact"].AddBinding("<Keyboard>/e");

            asset.AddActionMap(player);
            asset.AddActionMap(child);

            // Save
            var path = "Assets/InputSystem_Actions.asset";
            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            UnityEditor.AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("WWIII", "Created default Input Actions at Assets/InputSystem_Actions.asset", "OK");
#else
            EditorUtility.DisplayDialog("WWIII", "Input System not enabled.", "OK");
#endif
        }
    }
}

