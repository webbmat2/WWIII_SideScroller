using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Simple Input Actions setup for WWIII SideScroller
/// Supports Backbone controller, keyboard, and mobile touch for Unity 6
/// Compatible with iPhone 16 Pro and tvOS/AirPlay
/// </summary>
[System.Serializable]
public class PlayerInputActions : System.IDisposable
{
    private InputActionAsset asset;
    
    private readonly InputActionMap m_Player;
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Ability;
    private readonly InputAction m_Player_Pause;
    
    public struct PlayerActions
    {
        private PlayerInputActions m_Wrapper;
        public PlayerActions(PlayerInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @Ability => m_Wrapper.m_Player_Ability;
        public InputAction @Pause => m_Wrapper.m_Player_Pause;
    }
    
    public PlayerActions @Player => new PlayerActions(this);
    
    public PlayerInputActions()
    {
        asset = ScriptableObject.CreateInstance<InputActionAsset>();
        
        // Create Player action map
        m_Player = new InputActionMap("Player");
        asset.AddActionMap(m_Player);
        
        // Movement Action - Supports Backbone left stick, WASD, Arrow keys
        m_Player_Move = m_Player.AddAction("Move", binding: "<Gamepad>/leftStick");
        m_Player_Move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        m_Player_Move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        
        // Jump Action - Backbone A button, Space, Enter
        m_Player_Jump = m_Player.AddAction("Jump", binding: "<Gamepad>/buttonSouth");
        m_Player_Jump.AddBinding("<Keyboard>/space");
        m_Player_Jump.AddBinding("<Keyboard>/enter");
        m_Player_Jump.AddBinding("<Keyboard>/upArrow>");
        m_Player_Jump.AddBinding("<Keyboard>/w>");
        
        // Ability Action - Backbone X button, X key, Left Shift
        m_Player_Ability = m_Player.AddAction("Ability", binding: "<Gamepad>/buttonWest");
        m_Player_Ability.AddBinding("<Keyboard>/x");
        m_Player_Ability.AddBinding("<Keyboard>/leftShift");
        m_Player_Ability.AddBinding("<Keyboard>/c");
        
        // Pause Action - Backbone Menu button, Escape, P
        m_Player_Pause = m_Player.AddAction("Pause", binding: "<Gamepad>/start");
        m_Player_Pause.AddBinding("<Keyboard>/escape");
        m_Player_Pause.AddBinding("<Keyboard>/p");
        m_Player_Pause.AddBinding("<Gamepad>/select");
    }
    
    public void Dispose()
    {
        if (asset != null)
        {
            UnityEngine.Object.DestroyImmediate(asset);
        }
    }
    
    public void Enable()
    {
        asset.Enable();
    }
    
    public void Disable()
    {
        asset.Disable();
    }
}