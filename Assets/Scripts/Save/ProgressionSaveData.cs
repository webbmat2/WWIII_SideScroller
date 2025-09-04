using System;
using System.Collections.Generic;
using UnityEngine;

namespace WWIII.SideScroller.Save
{
    [Serializable]
    public class ProgressionSaveData
    {
        public int currentAgeIndex = 0;
        public List<string> collectedPhotoIds = new List<string>();
        public List<string> completedCutsceneIds = new List<string>();

        public bool HasPhoto(string id) => collectedPhotoIds != null && collectedPhotoIds.Contains(id);
    }
}

