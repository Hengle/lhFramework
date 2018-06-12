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
        int platform =System.Convert.ToInt32(dic["platform"]);
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
        foreach (var item in dic)
        {
            UnityEngine.Debug.Log(item.Key+"    "+item.Value);
        }
        if (dic.ContainsKey("currentLevel") && !string.IsNullOrEmpty(dic["currentLevel"]))
            QualitySettings.SetQualityLevel(System.Convert.ToInt32(dic["currentLevel"]));
        if (dic.ContainsKey("development") && !string.IsNullOrEmpty(dic["development"]))
            EditorUserBuildSettings.development = System.Convert.ToBoolean(System.Convert.ToInt32(dic["development"]));
        if (dic.ContainsKey("connectProfiler") && !string.IsNullOrEmpty(dic["connectProfiler"]))
            EditorUserBuildSettings.connectProfiler = System.Convert.ToBoolean(System.Convert.ToInt32(dic["connectProfiler"]));
        if (dic.ContainsKey("buildScriptsOnly") && !string.IsNullOrEmpty(dic["buildScriptsOnly"]))
            EditorUserBuildSettings.buildScriptsOnly = System.Convert.ToBoolean(System.Convert.ToInt32(dic["buildScriptsOnly"]));
        if (dic.ContainsKey("allowDebugging") && !string.IsNullOrEmpty(dic["allowDebugging"]))
            EditorUserBuildSettings.allowDebugging = System.Convert.ToBoolean(System.Convert.ToInt32(dic["allowDebugging"]));
        if (dic.ContainsKey("compressFilesInPackage") && !string.IsNullOrEmpty(dic["compressFilesInPackage"]))
            EditorUserBuildSettings.compressFilesInPackage = System.Convert.ToBoolean(System.Convert.ToInt32(dic["compressFilesInPackage"]));
        if (dic.ContainsKey("compressWithPsArc") && !string.IsNullOrEmpty(dic["compressWithPsArc"]))
            EditorUserBuildSettings.compressWithPsArc = System.Convert.ToBoolean(System.Convert.ToInt32(dic["compressWithPsArc"]));
        if (dic.ContainsKey("enableHeadlessMode") && !string.IsNullOrEmpty(dic["enableHeadlessMode"]))
            EditorUserBuildSettings.enableHeadlessMode = System.Convert.ToBoolean(System.Convert.ToInt32(dic["enableHeadlessMode"]));
        if (dic.ContainsKey("explicitDivideByZeroChecks") && !string.IsNullOrEmpty(dic["explicitDivideByZeroChecks"]))
            EditorUserBuildSettings.explicitDivideByZeroChecks = System.Convert.ToBoolean(System.Convert.ToInt32(dic["explicitDivideByZeroChecks"]));
        if (dic.ContainsKey("explicitNullChecks") && !string.IsNullOrEmpty(dic["explicitNullChecks"]))
            EditorUserBuildSettings.explicitNullChecks = System.Convert.ToBoolean(System.Convert.ToInt32(dic["explicitNullChecks"]));
        if (dic.ContainsKey("androidBuildSystem") && !string.IsNullOrEmpty(dic["androidBuildSystem"]))
            EditorUserBuildSettings.androidBuildSystem = (AndroidBuildSystem)System.Convert.ToInt32(dic["androidBuildSystem"]);
        if (dic.ContainsKey("androidBuildSubtarget") && !string.IsNullOrEmpty(dic["androidBuildSubtarget"]))
            EditorUserBuildSettings.androidBuildSubtarget = (MobileTextureSubtarget)System.Convert.ToInt32(dic["androidBuildSubtarget"]);
        if (dic.ContainsKey("androidDebugMinification") && !string.IsNullOrEmpty(dic["androidDebugMinification"]))
            EditorUserBuildSettings.androidDebugMinification = (AndroidMinification)System.Convert.ToInt32(dic["androidDebugMinification"]);
        if (dic.ContainsKey("androidReleaseMinification") && !string.IsNullOrEmpty(dic["androidReleaseMinification"]))
            EditorUserBuildSettings.androidReleaseMinification = (AndroidMinification)System.Convert.ToInt32(dic["androidReleaseMinification"]);
        if (dic.ContainsKey("androidDeviceSocketAddress") && !string.IsNullOrEmpty(dic["androidDeviceSocketAddress"]))
            EditorUserBuildSettings.androidDeviceSocketAddress = dic["androidDeviceSocketAddress"];
        if (dic.ContainsKey("iOSBuildConfigType") && !string.IsNullOrEmpty(dic["iOSBuildConfigType"]))
            EditorUserBuildSettings.iOSBuildConfigType = (iOSBuildType)System.Convert.ToInt32(dic["iOSBuildConfigType"]);

        if (dic.ContainsKey("companyName") && !string.IsNullOrEmpty(dic["companyName"]))
            PlayerSettings.companyName = dic["companyName"];
        if (dic.ContainsKey("productName") && !string.IsNullOrEmpty(dic["productName"]))
            PlayerSettings.productName = dic["productName"];
        if (dic.ContainsKey("applicationIdentifier") && !string.IsNullOrEmpty(dic["applicationIdentifier"]))
            PlayerSettings.applicationIdentifier = dic["applicationIdentifier"];
        if (dic.ContainsKey("colorSpace") && !string.IsNullOrEmpty(dic["colorSpace"]))
            PlayerSettings.colorSpace = (ColorSpace)System.Convert.ToInt32(dic["colorSpace"]);
        if (dic.ContainsKey("gpuSkinning") && !string.IsNullOrEmpty(dic["gpuSkinning"]))
            PlayerSettings.gpuSkinning = System.Convert.ToBoolean(System.Convert.ToInt32(dic["gpuSkinning"]));
        if (dic.ContainsKey("graphicsJobs") && !string.IsNullOrEmpty(dic["graphicsJobs"]))
            PlayerSettings.graphicsJobs = System.Convert.ToBoolean(System.Convert.ToInt32(dic["graphicsJobs"]));
        if (dic.ContainsKey("muteOtherAudioSources") && !string.IsNullOrEmpty(dic["muteOtherAudioSources"]))
            PlayerSettings.muteOtherAudioSources = System.Convert.ToBoolean(System.Convert.ToInt32(dic["muteOtherAudioSources"]));
        if (dic.ContainsKey("runInBackground") && !string.IsNullOrEmpty(dic["runInBackground"]))
            PlayerSettings.runInBackground = System.Convert.ToBoolean(System.Convert.ToInt32(dic["runInBackground"]));
        if (dic.ContainsKey("stripEngineCode") && !string.IsNullOrEmpty(dic["stripEngineCode"]))
            PlayerSettings.stripEngineCode = System.Convert.ToBoolean(System.Convert.ToInt32(dic["stripEngineCode"]));
        if (dic.ContainsKey("strippingLevel") && !string.IsNullOrEmpty(dic["strippingLevel"]))
            PlayerSettings.strippingLevel = (StrippingLevel)System.Convert.ToInt32(dic["strippingLevel"]);
        if (platform==(int)BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            if (dic.ContainsKey("androidIsGame") && !string.IsNullOrEmpty(dic["androidIsGame"]))
                PlayerSettings.Android.androidIsGame = System.Convert.ToBoolean(System.Convert.ToInt32(dic["androidIsGame"]));
            if (dic.ContainsKey("androidTVCompatibility") && !string.IsNullOrEmpty(dic["androidTVCompatibility"]))
                PlayerSettings.Android.androidTVCompatibility = System.Convert.ToBoolean(System.Convert.ToInt32(dic["androidTVCompatibility"]));
            if (dic.ContainsKey("blitType") && !string.IsNullOrEmpty(dic["blitType"]))
                PlayerSettings.Android.blitType = (AndroidBlitType)System.Convert.ToInt32(dic["blitType"]);
            if (dic.ContainsKey("bundleVersionCode") && !string.IsNullOrEmpty(dic["bundleVersionCode"]))
                PlayerSettings.Android.bundleVersionCode = System.Convert.ToInt32(dic["bundleVersionCode"]);
            if (dic.ContainsKey("disableDepthAndStencilBuffers") && !string.IsNullOrEmpty(dic["disableDepthAndStencilBuffers"]))
                PlayerSettings.Android.disableDepthAndStencilBuffers = System.Convert.ToBoolean(System.Convert.ToInt32(dic["disableDepthAndStencilBuffers"]));
            if (dic.ContainsKey("forceInternetPermission") && !string.IsNullOrEmpty(dic["forceInternetPermission"]))
                PlayerSettings.Android.forceInternetPermission = System.Convert.ToBoolean(System.Convert.ToInt32(dic["forceInternetPermission"]));
            if (dic.ContainsKey("forceSDCardPermission") && !string.IsNullOrEmpty(dic["forceSDCardPermission"]))
                PlayerSettings.Android.forceSDCardPermission = System.Convert.ToBoolean(System.Convert.ToInt32(dic["forceSDCardPermission"]));
            if (dic.ContainsKey("keystoreName") && !string.IsNullOrEmpty(dic["keystoreName"]))
                PlayerSettings.Android.keystoreName = Application.dataPath + dic["keystoreName"];
            if (dic.ContainsKey("keystorePass") && !string.IsNullOrEmpty(dic["keystorePass"]))
                PlayerSettings.Android.keystorePass = dic["keystorePass"];
            if (dic.ContainsKey("keyaliasName") && !string.IsNullOrEmpty(dic["keyaliasName"]))
                PlayerSettings.Android.keyaliasName =dic["keyaliasName"];
            if (dic.ContainsKey("keyaliasPass") && !string.IsNullOrEmpty(dic["keyaliasPass"]))
                PlayerSettings.Android.keyaliasPass = dic["keyaliasPass"];
            if (dic.ContainsKey("maxAspectRatio") && !string.IsNullOrEmpty(dic["maxAspectRatio"]))
                PlayerSettings.Android.maxAspectRatio = System.Convert.ToSingle(dic["maxAspectRatio"]);
            if (dic.ContainsKey("minSdkVersion") && !string.IsNullOrEmpty(dic["minSdkVersion"]))
                PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)System.Convert.ToInt32(dic["minSdkVersion"]);
            if (dic.ContainsKey("preferredInstallLocation") && !string.IsNullOrEmpty(dic["preferredInstallLocation"]))
                PlayerSettings.Android.preferredInstallLocation = (AndroidPreferredInstallLocation)System.Convert.ToInt32(dic["preferredInstallLocation"]);
            if (dic.ContainsKey("showActivityIndicatorOnLoading") && !string.IsNullOrEmpty(dic["showActivityIndicatorOnLoading"]))
                PlayerSettings.Android.showActivityIndicatorOnLoading = (AndroidShowActivityIndicatorOnLoading)System.Convert.ToInt32(dic["showActivityIndicatorOnLoading"]);
            if (dic.ContainsKey("splashScreenScale") && !string.IsNullOrEmpty(dic["splashScreenScale"]))
                PlayerSettings.Android.splashScreenScale = (AndroidSplashScreenScale)System.Convert.ToInt32(dic["splashScreenScale"]);
            if (dic.ContainsKey("targetDevice") && !string.IsNullOrEmpty(dic["targetDevice"]))
                PlayerSettings.Android.targetDevice = (AndroidTargetDevice)System.Convert.ToInt32(dic["targetDevice"]);
            if (dic.ContainsKey("targetSdkVersion") && !string.IsNullOrEmpty(dic["targetSdkVersion"]))
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)System.Convert.ToInt32(dic["targetSdkVersion"]);
            if (dic.ContainsKey("useAPKExpansionFiles") && !string.IsNullOrEmpty(dic["useAPKExpansionFiles"]))
                PlayerSettings.Android.useAPKExpansionFiles = System.Convert.ToBoolean(System.Convert.ToInt32(dic["useAPKExpansionFiles"]));

            string filepath = outputPath +".apk";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.Android, BuildOptions.None);
        }
        else if (platform == (int)BuildTarget.iOS)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            if (dic.ContainsKey("allowHTTPDownload") && !string.IsNullOrEmpty(dic["allowHTTPDownload"]))
                PlayerSettings.iOS.allowHTTPDownload = System.Convert.ToBoolean(System.Convert.ToInt32(dic["allowHTTPDownload"]));
            if (dic.ContainsKey("appInBackgroundBehavior") && !string.IsNullOrEmpty(dic["appInBackgroundBehavior"]))
                PlayerSettings.iOS.appInBackgroundBehavior = (iOSAppInBackgroundBehavior)System.Convert.ToInt32(dic["appInBackgroundBehavior"]);
            if (dic.ContainsKey("appleDeveloperTeamID") && !string.IsNullOrEmpty(dic["appleDeveloperTeamID"]))
                PlayerSettings.iOS.appleDeveloperTeamID = dic["appleDeveloperTeamID"];
            if (dic.ContainsKey("appleEnableAutomaticSigning") && !string.IsNullOrEmpty(dic["appleEnableAutomaticSigning"]))
                PlayerSettings.iOS.appleEnableAutomaticSigning = System.Convert.ToBoolean(System.Convert.ToInt32(dic["appleEnableAutomaticSigning"]));
            if (dic.ContainsKey("applicationDisplayName") && !string.IsNullOrEmpty(dic["applicationDisplayName"]))
                PlayerSettings.iOS.applicationDisplayName = dic["applicationDisplayName"];
            if (dic.ContainsKey("backgroundModes") && !string.IsNullOrEmpty(dic["backgroundModes"]))
                PlayerSettings.iOS.backgroundModes = (iOSBackgroundMode)System.Convert.ToInt32(dic["backgroundModes"]);
            if (dic.ContainsKey("buildNumber") && !string.IsNullOrEmpty(dic["buildNumber"]))
                PlayerSettings.iOS.buildNumber = dic["buildNumber"];
            if (dic.ContainsKey("cameraUsageDescription") && !string.IsNullOrEmpty(dic["cameraUsageDescription"]))
                PlayerSettings.iOS.cameraUsageDescription = dic["cameraUsageDescription"];
            if (dic.ContainsKey("forceHardShadowsOnMetal") && !string.IsNullOrEmpty(dic["forceHardShadowsOnMetal"]))
                PlayerSettings.iOS.forceHardShadowsOnMetal = System.Convert.ToBoolean(System.Convert.ToInt32(dic["forceHardShadowsOnMetal"]));
            if (dic.ContainsKey("iOSManualProvisioningProfileID") && !string.IsNullOrEmpty(dic["iOSManualProvisioningProfileID"]))
                PlayerSettings.iOS.iOSManualProvisioningProfileID = dic["iOSManualProvisioningProfileID"];
            if (dic.ContainsKey("locationUsageDescription") && !string.IsNullOrEmpty(dic["locationUsageDescription"]))
                PlayerSettings.iOS.locationUsageDescription = dic["locationUsageDescription"];
            if (dic.ContainsKey("microphoneUsageDescription") && !string.IsNullOrEmpty(dic["microphoneUsageDescription"]))
                PlayerSettings.iOS.microphoneUsageDescription = dic["microphoneUsageDescription"];
            if (dic.ContainsKey("prerenderedIcon") && !string.IsNullOrEmpty(dic["prerenderedIcon"]))
                PlayerSettings.iOS.prerenderedIcon = System.Convert.ToBoolean(System.Convert.ToInt32(dic["prerenderedIcon"]));
            if (dic.ContainsKey("requiresFullScreen") && !string.IsNullOrEmpty(dic["requiresFullScreen"]))
                PlayerSettings.iOS.requiresFullScreen = System.Convert.ToBoolean(System.Convert.ToInt32(dic["requiresFullScreen"]));
            if (dic.ContainsKey("requiresPersistentWiFi") && !string.IsNullOrEmpty(dic["requiresPersistentWiFi"]))
                PlayerSettings.iOS.requiresPersistentWiFi = System.Convert.ToBoolean(System.Convert.ToInt32(dic["requiresPersistentWiFi"]));
            if (dic.ContainsKey("scriptCallOptimization") && !string.IsNullOrEmpty(dic["scriptCallOptimization"]))
                PlayerSettings.iOS.scriptCallOptimization =(ScriptCallOptimizationLevel)System.Convert.ToInt32(dic["scriptCallOptimization"]);
            if (dic.ContainsKey("sdkVersion") && !string.IsNullOrEmpty(dic["sdkVersion"]))
                PlayerSettings.iOS.sdkVersion = (iOSSdkVersion)System.Convert.ToInt32(dic["sdkVersion"]);
            if (dic.ContainsKey("showActivityIndicatorOnLoading") && !string.IsNullOrEmpty(dic["showActivityIndicatorOnLoading"]))
                PlayerSettings.iOS.showActivityIndicatorOnLoading = (iOSShowActivityIndicatorOnLoading)System.Convert.ToInt32(dic["showActivityIndicatorOnLoading"]);
            if (dic.ContainsKey("statusBarStyle") && !string.IsNullOrEmpty(dic["statusBarStyle"]))
                PlayerSettings.iOS.statusBarStyle = (iOSStatusBarStyle)System.Convert.ToInt32(dic["statusBarStyle"]);
            if (dic.ContainsKey("targetDevice") && !string.IsNullOrEmpty(dic["targetDevice"]))
                PlayerSettings.iOS.targetDevice = (iOSTargetDevice)System.Convert.ToInt32(dic["targetDevice"]);
            if (dic.ContainsKey("targetOSVersionString") && !string.IsNullOrEmpty(dic["targetOSVersionString"]))
                PlayerSettings.iOS.targetOSVersionString = dic["targetOSVersionString"];

            string filepath = outputPath + ".ipa";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.iOS, BuildOptions.None);
        }
        else if (platform == (int)BuildTarget.StandaloneWindows64)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            string filepath = outputPath + ".exe";
            FileInfo fileInfo = new FileInfo(filepath);
            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();
            BuildPipeline.BuildPlayer(sceneList.ToArray(), filepath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        }
        else if (platform == (int)BuildTarget.StandaloneOSXIntel64)
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