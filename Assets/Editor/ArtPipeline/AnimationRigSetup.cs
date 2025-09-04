using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class AnimationRigSetup
    {
        public class Clips
        {
            public AnimationClip idle, walk, jump, interact;
        }

        public static Clips CreateFrameAnimationClips(string baseFolder, int ageYears)
        {
            var clips = new Clips();
            clips.idle = BuildClipFromFolder(Path.Combine(baseFolder, "Idle"), $"Jim_{ageYears}_Idle");
            clips.walk = BuildClipFromFolder(Path.Combine(baseFolder, "Walk"), $"Jim_{ageYears}_Walk", 8f);
            clips.jump = BuildClipFromFolder(Path.Combine(baseFolder, "Jump"), $"Jim_{ageYears}_Jump", 6f);
            clips.interact = BuildClipFromFolder(Path.Combine(baseFolder, "Interact"), $"Jim_{ageYears}_Interact", 6f);
            return clips;
        }

        private static AnimationClip BuildClipFromFolder(string folder, string clipName, float fps = 6f)
        {
            Directory.CreateDirectory(folder);
            var pngs = Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly);
            System.Array.Sort(pngs);
            var clip = new AnimationClip { frameRate = fps };

            var binding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[pngs.Length];
            for (int i = 0; i < pngs.Length; i++)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ToAssetPath(pngs[i]));
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = sprite
                };
            }
            if (keyframes.Length == 0)
            {
                // create a single still frame if empty
                keyframes = new[] { new ObjectReferenceKeyframe { time = 0f, value = null } };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            AssetDatabase.CreateAsset(clip, Path.Combine(folder, clipName + ".anim"));
            return clip;
        }

        public static RuntimeAnimatorController CreateAnimatorController(int ageYears, Clips clips, string outPath)
        {
            var controller = new AnimatorController();
            controller.name = $"Jim_{ageYears}_Controller";
            AssetDatabase.CreateAsset(controller, outPath);

            var rootStateMachine = controller.layers[0].stateMachine;
            var idle = rootStateMachine.AddState("Idle");
            idle.motion = clips.idle;
            var walk = rootStateMachine.AddState("Walk");
            walk.motion = clips.walk;
            var jump = rootStateMachine.AddState("Jump");
            jump.motion = clips.jump;
            var interact = rootStateMachine.AddState("Interact");
            interact.motion = clips.interact;

            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Interact", AnimatorControllerParameterType.Trigger);

            // Simple transitions
            var anyToInteract = rootStateMachine.AddAnyStateTransition(interact);
            anyToInteract.AddCondition(AnimatorConditionMode.If, 0f, "Interact");
            anyToInteract.hasExitTime = false; anyToInteract.duration = 0.05f;

            var idleToWalk = idle.AddTransition(walk);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.hasExitTime = false; idleToWalk.duration = 0.05f;

            var walkToIdle = walk.AddTransition(idle);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.hasExitTime = false; walkToIdle.duration = 0.05f;

            var anyToJump = rootStateMachine.AddAnyStateTransition(jump);
            anyToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            anyToJump.hasExitTime = false; anyToJump.duration = 0.05f;

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static string ToAssetPath(string fullPath)
        {
            return fullPath.Replace(Path.DirectorySeparatorChar, '/');
        }
    }
}

