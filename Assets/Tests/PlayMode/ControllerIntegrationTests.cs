using NUnit.Framework;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine;
using WWIII.SideScroller.Input;
using WWIII.SideScroller.Aging;

public class ControllerIntegrationTests
{
    [Test]
    public void AgeInputProfileSwitcher_NoCrash_When_NoActions()
    {
        var go = new GameObject("Switcher");
        var sw = go.AddComponent<AgeInputProfileSwitcher>();
        var mgrGo = new GameObject("AgeMgr");
        var mgr = mgrGo.AddComponent<AgeManager>();
        sw.ageManager = mgr;
        var p = ScriptableObject.CreateInstance<AgeProfile>();
        p.ageYears = 7; p.abilities.maxNumberOfJumps = 1;
        sw.SendMessage("Apply", p, SendMessageOptions.DontRequireReceiver);
        Assert.Pass();
    }
}

