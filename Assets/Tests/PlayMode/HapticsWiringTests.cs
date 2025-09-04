using NUnit.Framework;
using UnityEngine;
using WWIII.SideScroller.Input;
using WWIII.SideScroller.Integration.Corgi;
using MoreMountains.CorgiEngine;

public class HapticsWiringTests
{
    [Test]
    public void CorgiHaptics_NoException_On_CharacterEvents()
    {
        var go = new GameObject("H");
        go.AddComponent<CorgiHaptics>();
        // Should auto-create ControllerHapticsService on demand when events come
        MMCharacterEvent.Trigger(null, MMCharacterEventTypes.Jump);
        MMCharacterEvent.Trigger(null, MMCharacterEventTypes.Dash);
        MMCharacterEvent.Trigger(null, MMCharacterEventTypes.WallCling);
        MMCharacterEvent.Trigger(null, MMCharacterEventTypes.WallJump);
        Assert.Pass();
    }
}

