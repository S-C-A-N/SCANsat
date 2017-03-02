using UnityEditor;

public class Bundler
{
    [MenuItem("SCANsat/Build Bundles")]
    static void BuildAllAssetBundles()
    {
		BuildPipeline.BuildAssetBundles("AssetBundles", BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }
}
