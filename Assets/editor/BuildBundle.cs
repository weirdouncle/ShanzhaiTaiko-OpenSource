using UnityEditor;
using System;
using System.IO;

public class BuildBundle
{
    [MenuItem("Assets/BuildAssetBundles/Windows")]
    static void BuillAll()
    {
        string path = Environment.CurrentDirectory + "/AssetBundles";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
    }
}
