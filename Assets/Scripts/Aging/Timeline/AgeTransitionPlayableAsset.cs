using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using WWIII.SideScroller.Aging;

namespace WWIII.SideScroller.Aging.Timeline
{
    [Serializable]
    public class AgeTransitionPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        [Tooltip("Target age index within the AgeSet.")]
        public int targetAgeIndex = 0;

        [Tooltip("If true, plays the age's transition cutscene via the bound PlayableDirector.")]
        public bool playCutscene = true;

        [Tooltip("Optional explicit reference to the AgeManager. If left empty, behaviour will search in scene.")]
        public ExposedReference<AgeManager> ageManager;

        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AgeTransitionPlayableBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.targetAgeIndex = targetAgeIndex;
            behaviour.playCutscene = playCutscene;
            behaviour.ageManager = ageManager.Resolve(graph.GetResolver());
            return playable;
        }
    }
}
