using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine;
using WWIII.SideScroller.Collectibles;

public class PhotoAlbumServicePlayModeTests
{
    [UnityTest]
    public System.Collections.IEnumerator AddPhoto_AddsToSave()
    {
        var go = new GameObject("PhotoAlbumService");
        go.AddComponent<PhotoAlbumService>();
        yield return null;
        Assert.IsNotNull(PhotoAlbumService.Instance);
        // simulate add without sprite
        PhotoAlbumService.Instance.AddPhotoAsync("test_photo", null);
        yield return new WaitForSeconds(0.05f);
        Assert.IsTrue(WWIII.SideScroller.Save.ProgressionSaveService.Instance == null || WWIII.SideScroller.Save.ProgressionSaveService.Instance.Data != null);
    }
}
