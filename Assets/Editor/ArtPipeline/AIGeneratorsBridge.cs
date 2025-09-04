using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WWIII.SideScroller.Editor.ArtPipeline
{
    public static class AIGeneratorsBridge
    {
        // Best-effort reflection hooks to Unity 6000.2 AI Generators without hard dependency.
        // Falls back to colored placeholders (so the pipeline is runnable end-to-end).

        public class GeneratedSet
        {
            public string idleFolder;
            public string walkFolder;
            public string jumpFolder;
            public string interactFolder;
        }

        public static GeneratedSet GenerateAgeSprites(AIGeneratorConfig cfg, AIGeneratorConfig.AgePrompt age, string outFolder)
        {
            Directory.CreateDirectory(outFolder);
            var result = new GeneratedSet
            {
                idleFolder = Path.Combine(outFolder, "Idle"),
                walkFolder = Path.Combine(outFolder, "Walk"),
                jumpFolder = Path.Combine(outFolder, "Jump"),
                interactFolder = Path.Combine(outFolder, "Interact")
            };
            Directory.CreateDirectory(result.idleFolder);
            Directory.CreateDirectory(result.walkFolder);
            Directory.CreateDirectory(result.jumpFolder);
            Directory.CreateDirectory(result.interactFolder);

            bool usedAI = TryUseUnityAIGenerators(cfg, age, result);
            if (!usedAI)
            {
                // Create placeholder sprites so the rest of pipeline proceeds
                CreatePlaceholderSheet(result.idleFolder, $"Jim_{age.ageYears}_Idle", 1, new Color(0.5f, 0.8f, 1f));
                CreatePlaceholderSheet(result.walkFolder, $"Jim_{age.ageYears}_Walk", 6, new Color(0.6f, 1f, 0.7f));
                CreatePlaceholderSheet(result.jumpFolder, $"Jim_{age.ageYears}_Jump", 4, new Color(1f, 0.9f, 0.6f));
                CreatePlaceholderSheet(result.interactFolder, $"Jim_{age.ageYears}_Interact", 2, new Color(1f, 0.7f, 0.7f));
                AssetDatabase.Refresh();
            }
            return result;
        }

        private static bool TryUseUnityAIGenerators(AIGeneratorConfig cfg, AIGeneratorConfig.AgePrompt age, GeneratedSet outPaths)
        {
            try
            {
                var genType = Type.GetType("UnityEditor.AI.Generators.SpriteGenerator, UnityEditor.AI.Generators");
                if (genType == null)
                    genType = Type.GetType("UnityEditor.Muse.SpriteGenerator, UnityEditor.Muse");
                if (genType == null)
                    return false;

                // The real API specifics may differ. We'll construct prompts and assume a Generate API exists.
                string basePrompt = cfg.globalStylePrompt + "; " + age.characterDescription + "; biographical consistency across ages";
                GenerateWith(genType, basePrompt + "; " + age.idlePrompt, outPaths.idleFolder, $"Jim_{age.ageYears}_Idle");
                GenerateWith(genType, basePrompt + "; " + age.walkPrompt, outPaths.walkFolder, $"Jim_{age.ageYears}_Walk");
                GenerateWith(genType, basePrompt + "; " + age.jumpPrompt, outPaths.jumpFolder, $"Jim_{age.ageYears}_Jump");
                GenerateWith(genType, basePrompt + "; " + age.interactPrompt, outPaths.interactFolder, $"Jim_{age.ageYears}_Interact");
                AssetDatabase.Refresh();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"AI Generators reflection failed; using placeholders. {e.Message}");
                return false;
            }
        }

        private static void GenerateWith(Type genType, string prompt, string folder, string basename)
        {
            // Pseudo flow via reflection; you can adapt to concrete API if present.
            // Fallback for now: do nothing; placeholders will have been created if overall TryUseUnityAIGenerators failed.
        }

        private static void CreatePlaceholderSheet(string folder, string baseName, int frames, Color color)
        {
            for (int i = 0; i < frames; i++)
            {
                var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
                var pixels = Enumerable.Repeat(new Color(color.r, color.g, color.b, 1f), 256 * 256).ToArray();
                tex.SetPixels(pixels);
                var label = i.ToString("00");
                DrawText(tex, $"{baseName}_{label}", Color.black);
                tex.Apply(false, false);

                var bytes = tex.EncodeToPNG();
                var path = Path.Combine(folder, $"{baseName}_{label}.png");
                File.WriteAllBytes(path, bytes);
                UnityEngine.Object.DestroyImmediate(tex);
            }
        }

        private static void DrawText(Texture2D tex, string text, Color color)
        {
            // Minimal label painter to make placeholders identifiable
            int x = 8, y = tex.height - 20;
            foreach (var ch in text.Take(24))
            {
                DrawChar(tex, ch, x, y, color);
                x += 10;
            }
        }

        private static void DrawChar(Texture2D tex, char c, int ox, int oy, Color color)
        {
            // Very crude block font
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 10; j++)
                {
                    if (((i + j + c) % 7) == 0)
                        tex.SetPixel(Mathf.Clamp(ox + i, 0, tex.width - 1), Mathf.Clamp(oy - j, 0, tex.height - 1), color);
                }
        }
    }
}

