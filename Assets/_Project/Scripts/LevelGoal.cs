using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    [SerializeField] string nextSceneName = "";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        // Simple transition: reload if nextSceneName empty
        if (string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        else
            SceneManager.LoadScene(nextSceneName);
    }
}