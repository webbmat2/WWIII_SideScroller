using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

namespace WWIII.Core
{
    public class SceneTransitionManager : MonoBehaviour
    {
        [Header("Scene Transition Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private bool useAsyncLoading = true;
        
        public static SceneTransitionManager Instance { get; private set; }
        
        public bool IsTransitioning { get; private set; }
        
        public System.Action<string> OnSceneLoadStarted;
        public System.Action<string> OnSceneLoadCompleted;
        public System.Action<float> OnLoadingProgress;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void LoadScene(string sceneName)
        {
            if (IsTransitioning) return;
            
            if (useAsyncLoading)
            {
                StartCoroutine(LoadSceneAsync(sceneName));
            }
            else
            {
                LoadSceneImmediate(sceneName);
            }
        }
        
        private void LoadSceneImmediate(string sceneName)
        {
            IsTransitioning = true;
            OnSceneLoadStarted?.Invoke(sceneName);
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            
            OnSceneLoadCompleted?.Invoke(sceneName);
            IsTransitioning = false;
        }
        
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            IsTransitioning = true;
            OnSceneLoadStarted?.Invoke(sceneName);
            
            // Fade out current scene
            yield return StartCoroutine(FadeOut());
            
            // Load new scene asynchronously
            AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                OnLoadingProgress?.Invoke(progress);
                
                if (asyncLoad.progress >= 0.9f)
                {
                    break;
                }
                
                yield return null;
            }
            
            // Complete loading
            asyncLoad.allowSceneActivation = true;
            
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Fade in new scene
            yield return StartCoroutine(FadeIn());
            
            OnSceneLoadCompleted?.Invoke(sceneName);
            IsTransitioning = false;
        }
        
        private IEnumerator FadeOut()
        {
            // Simple fade logic - can be enhanced with UI fade overlay
            float elapsed = 0f;
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeOutDuration);
                
                // Apply fade effect here (UI overlay, camera fade, etc.)
                
                yield return null;
            }
        }
        
        private IEnumerator FadeIn()
        {
            // Simple fade logic - can be enhanced with UI fade overlay
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
                
                // Apply fade effect here (UI overlay, camera fade, etc.)
                
                yield return null;
            }
        }
        
        public void LoadBootScene()
        {
            LoadScene("Boot");
        }
        
        public void LoadMainMenu()
        {
            LoadScene("MainMenu");
        }
        
        public void LoadLevelSelect()
        {
            LoadScene("Level_Select");
        }
        
        public void LoadTutorialLevel()
        {
            LoadScene("L1_Tutorial");
        }
        
        public void LoadLevel(int levelNumber)
        {
            string sceneName = levelNumber switch
            {
                1 => "L1_Tutorial",
                2 => "L2_Stub",
                3 => "L3_Stub", 
                4 => "L4_Stub",
                5 => "L5_Stub",
                _ => "L1_Tutorial"
            };
            
            LoadScene(sceneName);
        }
        
        public void RestartCurrentLevel()
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            LoadScene(currentScene);
        }
        
        public void QuitToMainMenu()
        {
            LoadMainMenu();
        }
        
        public string GetCurrentSceneName()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
        
        public bool IsInLevel()
        {
            string sceneName = GetCurrentSceneName();
            return sceneName.StartsWith("L") && (sceneName.Contains("Tutorial") || sceneName.Contains("Stub"));
        }
        
        public bool IsInMenu()
        {
            string sceneName = GetCurrentSceneName();
            return sceneName == "MainMenu" || sceneName == "Level_Select" || sceneName == "Boot";
        }
    }
}