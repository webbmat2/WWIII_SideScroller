using UnityEngine.Timeline;

namespace WWIII.SideScroller.Aging.Timeline
{
    [TrackColor(0.8f, 0.5f, 0.2f)]
    [TrackBindingType(typeof(WWIII.SideScroller.Aging.AgeManager))]
    [TrackClipType(typeof(AgeTransitionPlayableAsset))]
    public class AgeTransitionTrack : TrackAsset
    {
    }
}

