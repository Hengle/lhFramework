using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public static class BundleManager 
{
    private static string bundleFolder = "Bundle";
    private static string sceneFolder = "Scene";
    private static string bundleSourceFolder = Application.dataPath + "/Resources";
    private static string sceneSourceFolder = Application.dataPath + "/Scene";
    private static string bundleStuff = ".bundle";
    private static string sceneStuff = ".unity3d";
    private static string bundleOutputFolder = "Assets/StreamingAssets" ;
    private static string sceneOutputFolder = Application.streamingAssetsPath;

    [MenuItem("Tools/AssetBundle/BuildAll %t")]
    public static void Build(){
        BuildAssetBundle();
        BuildScene();
    }

    [MenuItem("Tools/AssetBundle/BundleBuild %t")]
    static void BuildAssetBundle()
    {
        if (EditorApplication.isCompiling) return;
        //ClearAssetBundlesName();
        PackBundle(bundleSourceFolder);
        string putPath =Path.Combine( Path.Combine(bundleOutputFolder, EditorUserBuildSettings.activeBuildTarget.ToString()), bundleFolder);
        if (!Directory.Exists(putPath))
        {
            Directory.CreateDirectory(putPath);
        }
        BuildPipeline.BuildAssetBundles(putPath, 0, EditorUserBuildSettings.activeBuildTarget);
        //ClearAssetBundlesName();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Information", "Build success!", "Ok");
    }
    [MenuItem("Tools/AssetBundle/SceneBuild")]
    static void BuildScene()
    {
        string[] levels = PackScene(sceneSourceFolder);
        for (int i = 0; i < levels.Length; i++)
        {
            string putPath =Path.Combine( Path.Combine(sceneOutputFolder, EditorUserBuildSettings.activeBuildTarget.ToString()),sceneFolder);
            string p = levels[i].Replace("Assets/Scene", "").Replace(".unity", "");
            string locationName = putPath  + p+ sceneStuff;
            FileInfo info = new FileInfo(locationName);
            if (!info.Directory.Exists)
            {
                info.Directory.Create();
            }
            string error = BuildPipeline.BuildPlayer(new string[] { levels[i] }, locationName, EditorUserBuildSettings.activeBuildTarget, BuildOptions.BuildAdditionalStreamedScenes);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Information", "Build success!", "Ok");
    }
    [MenuItem("Tools/AssetBundle/RemoveBundleName")]
    static void RemoveBundleName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;
        string[] oldAssetBundleNames = new string[length];
        for (int i = 0; i < length; i++)
        {
            oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
        }

        for (int j = 0; j < oldAssetBundleNames.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
        }
        EditorUtility.DisplayDialog("Information", "Clear success!", "Ok");
    }
    static void PackBundle(string source)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                if (files[i].Name.ToLower().Contains("_null")) continue;
                PackBundle(files[i].FullName);
            }
            else
            {
                if (!files[i].Name.EndsWith(".meta"))
                {
                    SetBundleConfig(files[i].FullName);
                }
            }
        }
    }
    static string[] PackScene(string source)
    {
        string[] files = Directory.GetFiles(source, "*.unity", SearchOption.AllDirectories);
        List<string> levels = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            file = "Assets" + file.Substring(Application.dataPath.Length);
            levels.Add(file);
        }
        return levels.ToArray();
    }
    static void SetBundleConfig(string source)
    {
        source = source.Replace("\\", "/");
        string _assetPath = "Assets" + source.Substring(Application.dataPath.Length);
        string _assetPath2 = source.Substring(Application.dataPath.Length + 1);

        AssetImporter assetImporter = AssetImporter.GetAtPath(_assetPath);
        string assetName = _assetPath2.Substring(_assetPath2.IndexOf("/") + 1);
        assetName = assetName.Replace(Path.GetExtension(assetName), bundleStuff);
        assetImporter.assetBundleName = assetName;
        assetImporter.assetBundleVariant = "";
    }
}