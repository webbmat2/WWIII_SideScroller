using System;
using UnityEngine;
using UnityEngine.Playables;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Aging.Timeline
{
    public class AgeTransitionPlayableBehaviour : PlayableBehaviour
    {
        [NonSerialized] public int targetAgeIndex;
        [NonSerialized] public bool playCutscene = true;
        [NonSerialized] public AgeManager ageManager;

        private bool _fired;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (_fired) return;
            _fired = true;

            var director = playable.GetGraph().GetResolver() as PlayableDirector;
            var mgr = ageManager != null
                ? ageManager
                : (director != null ? director.GetComponent<AgeManager>() : null) ?? UnityEngine.Object.FindFirstObjectByType<AgeManager>();
            if (mgr == null)
            {
                Debug.LogWarning("AgeTransitionPlayableBehaviour: No AgeManager found.");
                return;
            }

            _ = mgr.RequestAgeIndexAsync(targetAgeIndex, playCutscene);
        }
    }
}
