using System;
using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.Level
{
    [CreateAssetMenu(menuName = "WWIII/Level/Spawn Spec", fileName = "LevelSpawnSpec")]
    public class LevelSpawnSpec : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string type; // Enemy, PowerUp, Collectible
            public string designId;
            public Vector2 position;
            public int count = 1;
        }

        public string levelDesignId;
        public List<Entry> entries = new List<Entry>();
    }
}
