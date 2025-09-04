using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace WWIII.SideScroller.Audio.Editor
{
    public class AudioImportOptimizer : AssetPostprocessor
    {
        void OnPreprocessAudio()
        {
            if (assetImporter is not AudioImporter importer) return;

            var isMusic = assetPath.StartsWith("Assets/Audio/Music/");
            var isSfx = assetPath.StartsWith("Assets/Audio/SFX/");

            var settings = importer.defaultSampleSettings;
            settings.loadType = isMusic ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = isMusic ? 0.7f : 0.9f; // Music 70%, SFX 90%
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            // Preload setting moved to sample settings in 2021.2+
#if UNITY_2021_2_OR_NEWER
            settings.preloadAudioData = true;
#else
            importer.preloadAudioData = true;
#endif
            importer.defaultSampleSettings = settings;
            importer.forceToMono = false;
            importer.loadInBackground = true;

            // iOS override settings
            var iosSettings = settings;
            iosSettings.quality = isMusic ? 0.6f : 0.8f; // iOS Music 60%, SFX 80%
#if UNITY_2021_2_OR_NEWER
            iosSettings.preloadAudioData = true;
#endif
            importer.SetOverrideSampleSettings("iOS", iosSettings);
        }

        [MenuItem("WWIII/Audio/Apply Import Settings To Selection")]
        private static void ApplyToSelection()
        {
            var guids = Selection.assetGUIDs;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer == null) continue;

                var isMusic = path.StartsWith("Assets/Audio/Music/");
                var isSfx = path.StartsWith("Assets/Audio/SFX/");
                var settings = importer.defaultSampleSettings;
                settings.loadType = isMusic ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = isMusic ? 0.7f : 0.9f;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                // Preload setting moved to sample settings in 2021.2+
#if UNITY_2021_2_OR_NEWER
                settings.preloadAudioData = true;
#else
                importer.preloadAudioData = true;
#endif
                importer.defaultSampleSettings = settings;
                importer.forceToMono = false;
                importer.loadInBackground = true;

                // iOS override
                var iosSettings = settings;
                iosSettings.quality = isMusic ? 0.6f : 0.8f;
#if UNITY_2021_2_OR_NEWER
                iosSettings.preloadAudioData = true;
#endif
                importer.SetOverrideSampleSettings("iOS", iosSettings);

                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
