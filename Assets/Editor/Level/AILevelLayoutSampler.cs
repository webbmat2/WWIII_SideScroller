using System.Security.Cryptography;
using System.Text;

namespace WWIII.SideScroller.Editor.Level
{
    public static class AILevelLayoutSampler
    {
        public static int SeedFromPrompt(string prompt)
        {
            using var sha1 = SHA1.Create();
            var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(prompt ?? ""));
            // fold into 32-bit int
            int seed = (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
            return seed;
        }
    }
}

