using NUnit.Framework;
using UnityEngine.TestTools;
using WWIII.SideScroller.Save;

public class ProgressionSaveServiceTests
{
    [Test]
    public void ProgressionSaveData_HasPhoto_Works()
    {
        var d = new ProgressionSaveData();
        Assert.IsFalse(d.HasPhoto("x"));
        d.collectedPhotoIds.Add("x");
        Assert.IsTrue(d.HasPhoto("x"));
    }
}
