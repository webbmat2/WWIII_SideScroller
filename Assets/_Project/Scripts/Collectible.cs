using UnityEngine;

namespace WWIII
{
    public class Collectible : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            Score.Add(1);
            Destroy(gameObject);
        }
    }

    // Minimal in-file score tracker
    internal static class Score
    {
        static int _value;
        public static int Value => _value;
        public static void Add(int v)
        {
            _value += v;
            Debug.Log("Score: " + _value);
        }
    }
}