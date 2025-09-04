using System;
using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    [CreateAssetMenu(menuName = "WWIII/Art Pipeline/AI Generator Config", fileName = "AIGeneratorConfig")]
    public class AIGeneratorConfig : ScriptableObject
    {
        [TextArea(3,6)]
        public string globalStylePrompt =
            "Side-scrolling platformer character, clean silhouettes, readable shapes, consistent lighting, flat colors with subtle gradients, modern cartoon realism, neutral palette, mobile-friendly 2D URP-ready.";

        [Serializable]
        public class AgePrompt
        {
            public string label = "Age 7";
            public int ageYears = 7;
            [TextArea(3,6)] public string characterDescription;
            [TextArea(2,4)] public string idlePrompt = "Idle pose, relaxed, facing right, full body";
            [TextArea(2,4)] public string walkPrompt = "Walk cycle frames, 6 frames, facing right";
            [TextArea(2,4)] public string jumpPrompt = "Jump frames (crouch, rise, apex, fall), facing right";
            [TextArea(2,4)] public string interactPrompt = "Interaction pose, reaching or picking up, facing right";
        }

        public List<AgePrompt> ages = new List<AgePrompt>
        {
            new AgePrompt{ label = "Age 7", ageYears = 7, characterDescription = "Jim as a child (7), small stature, rounder features, simple clothes (shirt + shorts), sneakers" },
            new AgePrompt{ label = "Age 11", ageYears = 11, characterDescription = "Jim preteen (11), slightly taller, more confident posture, simple hoodie, sneakers" },
            new AgePrompt{ label = "Age 14", ageYears = 14, characterDescription = "Jim teen (14), longer limbs, casual outfit (t-shirt, jeans), trainers" },
            new AgePrompt{ label = "Age 17", ageYears = 17, characterDescription = "Jim young adult (17), defined silhouette, jacket, jeans, boots" },
            new AgePrompt{ label = "Age 21", ageYears = 21, characterDescription = "Jim adult (21), mature posture, fitted jacket, neutral tones, boots" },
        };

        [Header("Output Settings")] public string outputRoot = "Assets/WWIII/Art/Jim";
        public string atlasRoot = "Assets/WWIII/Art/Jim/Atlases";
    }
}

