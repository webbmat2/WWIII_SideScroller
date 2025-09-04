using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class TimelineSetup
    {
        public static TimelineAsset CreateAgeTransitionTimeline(int targetAgeIndex, string outPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? "Assets");
            var timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            AssetDatabase.CreateAsset(timeline, outPath);

            // Basic timeline with a single AgeTransition clip
            var track = timeline.CreateTrack<WWIII.SideScroller.Aging.Timeline.AgeTransitionTrack>(null, "AgeTransition");
            var clip = track.CreateDefaultClip();
            clip.displayName = $"To Age Index {targetAgeIndex}";
            var asset = clip.asset as WWIII.SideScroller.Aging.Timeline.AgeTransitionPlayableAsset;
            if (asset != null)
            {
                asset.targetAgeIndex = targetAgeIndex;
                asset.playCutscene = false; // this timeline is the cutscene itself
            }
            AssetDatabase.SaveAssets();
            return timeline;
        }
    }
}

