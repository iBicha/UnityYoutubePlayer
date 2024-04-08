using UnityEditor;

namespace YoutubePlayer.Editor
{
    public class PackageUtils
    {
        [MenuItem("Utils/Copy Samples To Package")]
        static void CopySamplesToPackage()
        {
            // get version of package
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.ibicha.youtube-player/package.json");
            var version = packageInfo.version;

            FileUtil.DeleteFileOrDirectory("Packages/com.ibicha.youtube-player/Samples~");
            FileUtil.CopyFileOrDirectory($"Assets/Samples/Youtube Player/{version}", "Packages/com.ibicha.youtube-player/Samples~");
        }
    }
}
