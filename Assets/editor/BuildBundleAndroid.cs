using UnityEditor;
using System;
using System.IO;

public class BuildBundleAndroid
{
    [MenuItem("Assets/BuildAssetBundles/Android")]
    static void BuillAll()
    {
        string path = Environment.CurrentDirectory + "/AssetBundles/Andorid";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }
}
