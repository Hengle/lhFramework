using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public static class AutoBuild
{
    [MenuItem("Tools/AutoBuild/BuildPackage")]
    public static void BuildPackage(){

        string[] args=System.Environment.GetCommandLineArgs();
        //for (int i = 0; i < args.Length; i++)
        //{
        //    Debug.Log(args[i] +"   "+i);
        //}
        string version = args[9];
        string platform = args[10];
        string outputPath = args[11];//注意是相对路径  如  "./AutoBuild/Output/201805051202.apk"
        string mode = args[12];
        //Debug.Log("version:"+version);
        //Debug.Log("platform:" + platform);
        //Debug.Log("outputPath:" + outputPath);
        List<string> sceneList = new List<string>();
        EditorBuildSettingsScene[] temp = EditorBuildSettings.scenes;
        for (int i = 0, iMax = temp.Length; i < iMax; ++i)
            sceneList.Add(temp[i].path);
        string curTime = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm");
        if (platform.ToLower()=="android")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            string filepath = outputPath +".apk";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.Android, BuildOptions.None);
        }
        else if (platform.ToLower()=="ios")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            string filepath = outputPath + ".ipa";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.iOS, BuildOptions.None);
        }
        else if (platform.ToLower() == "standard_win")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            string filepath = outputPath + ".exe";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        }
        else if (platform.ToLower() == "standard_mac")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSXIntel64);
            string filepath = outputPath + ".exe";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.StandaloneOSXIntel64, BuildOptions.None);
        }
    }
    [MenuItem("Tools/AudoBuild/BuildSource")]
    public static void BuildSources(){
        
    }
}