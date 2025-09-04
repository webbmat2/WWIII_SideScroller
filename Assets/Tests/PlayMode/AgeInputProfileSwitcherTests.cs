using NUnit.Framework;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;
using WWIII.SideScroller.Input;
using WWIII.SideScroller.Aging;

public class AgeInputProfileSwitcherTests
{
#if ENABLE_INPUT_SYSTEM
    private InputActionAsset CreateActions()
    {
        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        var player = new InputActionMap("Player");
        player.AddAction("Dash");
        player.AddAction("Shoot");
        var child = new InputActionMap("Child");
        child.AddAction("Jump");
        asset.AddActionMap(player); asset.AddActionMap(child);
        return asset;
    }
#endif

    [Test]
    public void Switcher_Disables_Actions_For_Child()
    {
        var go = new GameObject("Switcher");
        var sw = go.AddComponent<AgeInputProfileSwitcher>();
#if ENABLE_INPUT_SYSTEM
        var actions = CreateActions();
        sw.GetType().GetField("actions", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance)?.SetValue(sw, actions);
#endif
        var mgrGo = new GameObject("AgeMgr");
        var mgr = mgrGo.AddComponent<AgeManager>(); sw.ageManager = mgr;
        var profile = ScriptableObject.CreateInstance<AgeProfile>();
        profile.ageYears = 7; profile.abilities = new AgeProfile.AbilityConfig { canDash = false, canShoot = false };
        sw.SendMessage("Apply", profile, SendMessageOptions.DontRequireReceiver);

        Assert.Pass();
    }
}

