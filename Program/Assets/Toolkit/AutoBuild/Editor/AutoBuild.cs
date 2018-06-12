using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

public static class AutoBuild
{
    [MenuItem("Tools/AutoBuild/BuildPackage")]
    public static void BuildPackage(){

        var dic = GetCommandArgs();
        string[] args=System.Environment.GetCommandLineArgs();
        string version = dic["version"];
        string platform = dic["platform"];
        string outputPath = dic["outputPath"];//注意是相对路径  如  "./AutoBuild/Output/201805051202.apk"
        string mode = dic["mode"];
        List<string> sceneList = new List<string>();
        EditorBuildSettingsScene[] temp = EditorBuildSettings.scenes;
        for (int i = 0, iMax = temp.Length; i < iMax; ++i)
        {
            var t = temp[i];
            if (t == null || t.enabled == false) continue;
            sceneList.Add(t.path);
        }
        string curTime = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm");
        if (dic.ContainsKey("companyName"))
            PlayerSettings.companyName = dic["companyName"];
        if (dic.ContainsKey("productName"))
            PlayerSettings.productName = dic["productName"];
        if (dic.ContainsKey("applicationIdentifier"))
            PlayerSettings.applicationIdentifier = dic["applicationIdentifier"];
        if (dic.ContainsKey("colorSpace"))
            PlayerSettings.colorSpace = (ColorSpace)System.Convert.ToInt32(dic["colorSpace"]);
        if (dic.ContainsKey("gpuSkinning"))
            PlayerSettings.gpuSkinning = System.Convert.ToBoolean(dic["gpuSkinning"]);
        if (dic.ContainsKey("graphicsJobs"))
            PlayerSettings.graphicsJobs = System.Convert.ToBoolean(dic["graphicsJobs"]);
        if (dic.ContainsKey("muteOtherAudioSources"))
            PlayerSettings.muteOtherAudioSources = System.Convert.ToBoolean(dic["muteOtherAudioSources"]);
        if (dic.ContainsKey("runInBackground"))
            PlayerSettings.runInBackground = System.Convert.ToBoolean(dic["runInBackground"]);
        if (dic.ContainsKey("stripEngineCode"))
            PlayerSettings.stripEngineCode = System.Convert.ToBoolean(dic["stripEngineCode"]);
        if (dic.ContainsKey("strippingLevel"))
            PlayerSettings.strippingLevel = (StrippingLevel)System.Convert.ToInt32(dic["strippingLevel"]);
        if (platform.ToLower()=="android")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            if (dic.ContainsKey("androidIsGame"))
                PlayerSettings.Android.androidIsGame = System.Convert.ToBoolean(dic["androidIsGame"]);
            if (dic.ContainsKey("androidTVCompatibility"))
                PlayerSettings.Android.androidTVCompatibility = System.Convert.ToBoolean(dic["androidTVCompatibility"]);
            if (dic.ContainsKey("blitType"))
                PlayerSettings.Android.blitType = (AndroidBlitType)System.Convert.ToInt32(dic["blitType"]);
            if (dic.ContainsKey("bundleVersionCode"))
                PlayerSettings.Android.bundleVersionCode = System.Convert.ToInt32(dic["bundleVersionCode"]);
            if (dic.ContainsKey("disableDepthAndStencilBuffers"))
                PlayerSettings.Android.disableDepthAndStencilBuffers = System.Convert.ToBoolean(dic["disableDepthAndStencilBuffers"]);
            if (dic.ContainsKey("forceInternetPermission"))
                PlayerSettings.Android.forceInternetPermission = System.Convert.ToBoolean(dic["forceInternetPermission"]);
            if (dic.ContainsKey("forceSDCardPermission"))
                PlayerSettings.Android.forceSDCardPermission = System.Convert.ToBoolean(dic["forceSDCardPermission"]);
            if (dic.ContainsKey("keystoreName"))
                PlayerSettings.Android.keystoreName = Application.dataPath + dic["keystoreName"];
            if (dic.ContainsKey("keystorePass"))
                PlayerSettings.Android.keystorePass = dic["keystorePass"];
            if (dic.ContainsKey("keyaliasName"))
                PlayerSettings.Android.keyaliasName =dic["keyaliasName"];
            if (dic.ContainsKey("keyaliasPass"))
                PlayerSettings.Android.keyaliasPass = dic["keyaliasPass"];
            if (dic.ContainsKey("maxAspectRatio"))
                PlayerSettings.Android.maxAspectRatio = System.Convert.ToSingle(dic["maxAspectRatio"]);
            if (dic.ContainsKey("minSdkVersion"))
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)System.Convert.ToInt32(dic["minSdkVersion"]);
            if (dic.ContainsKey("preferredInstallLocation"))
                PlayerSettings.Android.preferredInstallLocation = (AndroidPreferredInstallLocation)System.Convert.ToInt32(dic["preferredInstallLocation"]);
            if (dic.ContainsKey("showActivityIndicatorOnLoading"))
                PlayerSettings.Android.showActivityIndicatorOnLoading = (AndroidShowActivityIndicatorOnLoading)System.Convert.ToInt32(dic["showActivityIndicatorOnLoading"]);
            if (dic.ContainsKey("splashScreenScale"))
                PlayerSettings.Android.splashScreenScale = (AndroidSplashScreenScale)System.Convert.ToInt32(dic["splashScreenScale"]);
            if (dic.ContainsKey("targetDevice"))
                PlayerSettings.Android.targetDevice = (AndroidTargetDevice)System.Convert.ToInt32(dic["targetDevice"]);
            if (dic.ContainsKey("targetSdkVersion"))
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)System.Convert.ToInt32(dic["targetSdkVersion"]);
            if (dic.ContainsKey("useAPKExpansionFiles"))
                PlayerSettings.Android.useAPKExpansionFiles = System.Convert.ToBoolean(dic["useAPKExpansionFiles"]);

            string filepath = outputPath +".apk";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.Android, BuildOptions.None);
        }
        else if (platform.ToLower()=="ios")
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            if (dic.ContainsKey("allowHTTPDownload"))
                PlayerSettings.iOS.allowHTTPDownload = System.Convert.ToBoolean(dic["allowHTTPDownload"]);
            if (dic.ContainsKey("appInBackgroundBehavior"))
                PlayerSettings.iOS.appInBackgroundBehavior = (iOSAppInBackgroundBehavior)System.Convert.ToInt32(dic["appInBackgroundBehavior"]);
            if (dic.ContainsKey("appleDeveloperTeamID"))
                PlayerSettings.iOS.appleDeveloperTeamID = dic["appleDeveloperTeamID"];
            if (dic.ContainsKey("appleEnableAutomaticSigning"))
                PlayerSettings.iOS.appleEnableAutomaticSigning = System.Convert.ToBoolean(dic["appleEnableAutomaticSigning"]);
            if (dic.ContainsKey("applicationDisplayName"))
                PlayerSettings.iOS.applicationDisplayName = dic["applicationDisplayName"];
            if (dic.ContainsKey("backgroundModes"))
                PlayerSettings.iOS.backgroundModes = (iOSBackgroundMode)System.Convert.ToInt32(dic["backgroundModes"]);
            if (dic.ContainsKey("buildNumber"))
                PlayerSettings.iOS.buildNumber = dic["buildNumber"];
            if (dic.ContainsKey("cameraUsageDescription"))
                PlayerSettings.iOS.cameraUsageDescription = dic["cameraUsageDescription"];
            if (dic.ContainsKey("forceHardShadowsOnMetal"))
                PlayerSettings.iOS.forceHardShadowsOnMetal = System.Convert.ToBoolean(dic["forceHardShadowsOnMetal"]);
            if (dic.ContainsKey("iOSManualProvisioningProfileID"))
                PlayerSettings.iOS.iOSManualProvisioningProfileID = dic["iOSManualProvisioningProfileID"];
            if (dic.ContainsKey("locationUsageDescription"))
                PlayerSettings.iOS.locationUsageDescription = dic["locationUsageDescription"];
            if (dic.ContainsKey("microphoneUsageDescription"))
                PlayerSettings.iOS.microphoneUsageDescription = dic["microphoneUsageDescription"];
            if (dic.ContainsKey("prerenderedIcon"))
                PlayerSettings.iOS.prerenderedIcon = System.Convert.ToBoolean(dic["prerenderedIcon"]);
            if (dic.ContainsKey("requiresFullScreen"))
                PlayerSettings.iOS.requiresFullScreen = System.Convert.ToBoolean(dic["requiresFullScreen"]);
            if (dic.ContainsKey("requiresPersistentWiFi"))
                PlayerSettings.iOS.requiresPersistentWiFi = System.Convert.ToBoolean(dic["requiresPersistentWiFi"]);
            if (dic.ContainsKey("scriptCallOptimization"))
                PlayerSettings.iOS.scriptCallOptimization =(ScriptCallOptimizationLevel)System.Convert.ToInt32(dic["scriptCallOptimization"]);
            if (dic.ContainsKey("sdkVersion"))
                PlayerSettings.iOS.sdkVersion = (iOSSdkVersion)System.Convert.ToInt32(dic["sdkVersion"]);
            if (dic.ContainsKey("showActivityIndicatorOnLoading"))
                PlayerSettings.iOS.showActivityIndicatorOnLoading = (iOSShowActivityIndicatorOnLoading)System.Convert.ToInt32(dic["showActivityIndicatorOnLoading"]);
            if (dic.ContainsKey("statusBarStyle"))
                PlayerSettings.iOS.statusBarStyle = (iOSStatusBarStyle)System.Convert.ToInt32(dic["statusBarStyle"]);
            if (dic.ContainsKey("targetDevice"))
                PlayerSettings.iOS.targetDevice = (iOSTargetDevice)System.Convert.ToInt32(dic["targetDevice"]);
            if (dic.ContainsKey("targetOSVersionString"))
                PlayerSettings.iOS.targetOSVersionString = dic["targetOSVersionString"];

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
    private static Dictionary<string,string> GetCommandArgs()
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains("="))
            {
                var split = args[i].Split('=');
                dic.Add(split[0], split[1]);
            }
        }
        return dic;
    }
}