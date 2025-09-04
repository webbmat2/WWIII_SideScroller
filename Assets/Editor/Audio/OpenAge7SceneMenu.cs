using UnityEditor;
using UnityEditor.SceneManagement;

namespace WWIII.SideScroller.Editor.Audio
{
    public static class OpenAge7SceneMenu
    {
        [MenuItem("WWIII/Audio/Open BioLevel_Age7 Scene")]
        public static void OpenAge7()
        {
            EditorSceneManager.OpenScene("Assets/WWIII/Scenes/BioLevel_Age7.unity");
        }
    }
}

