#!/bin/sh
echo "start"
#这里就可以拿到jenkins传递进来的参数了
#把所有=后面的参数取出来
if test svn
then
echo "has svn"
svn update
fi
for a in $*
do
r=`echo $a | sed "s/--//g"`
eval $r
done

if [[ -z $platform || -z $version || -z $mode ]];
then
echo "error:must has platform version mode"
exit 0
fi

. ./read_ini.sh
read_ini properties.ini

dirname="pack-$platform-"$(date +%Y_%m_%d_%H_%M)
absoluteOutputPath=$INI__executePath__rootPath/AutoBuild/Output/$platform/$dirname
relateOutputPath=../AutoBuild/Output/$platform/$dirname
#运行时程序项目路径
programPath=$INI__executePath__rootPath/Program
#编辑器资源项目路径
artPath=$INI__executePath__rootPath/Art/$platform
#uiPath=$rootPath/UI
#audioPath=$rootPath/Audio

editor_maintainer_filepath=$absoluteOutputPath/maintainer.txt

if [[ ! -d $absoluteOutputPath ]];
then
mkdir -p $absoluteOutputPath;
else
rm -r $absoluteOutputPath;
mkdir -p $absoluteOutputPath;
fi

echo "version = $version"
echo "platform = $platform"
echo "mode = $mode"
echo "programPath = $programPath"
echo "artPath = $artPath"
echo "absoluteOutputPath = $absoluteOutputPath"
echo "relateOutputPath = $relateOutputPath"
echo "dirname = $dirname"
echo "package-------------------->  art"
#美术资源监测。  打开unity3d  执行AutoBuild.Build 方法。
$INI__executePath__unityPath \
-quit -batchmode \
-projectPath $artPath \
-logFile "$absoluteOutputPath/proj_art.log" \
-executeMethod AutoBuild.BuildPackage "$editor_maintainer_filepath" "$mode"
echo "package-------------------->  ui"

echo "package-------------------->  program"
#程序打包  unity产生log就写在tmp/1.log里面，比如Debug.Log和Unity编辑器产生的。
if [ $platform == "Android" ]; then
$INI__executePath__unityPath \
-quit -batchmode \
-projectPath $programPath \
-logFile "$absoluteOutputPath/proj_program.log" \
-executeMethod AutoBuild.BuildPackage \
"version=$version" \
"platform=$platform" \
"outputPath=$relateOutputPath/package" \
"mode=$mode" \
"companyName=$INI_playerSettings__companyName" \
"productName=$INI_playerSettings__productName" \
"applicationIdentifier=$INI_playerSettings__applicationIdentifier" \
"colorSpace=$INI_playerSettings__colorSpace" \
"gpuSkinning=$INI_playerSettings__gpuSkinning" \
"graphicsJobs=$INI_playerSettings__graphicsJobs" \
"muteOtherAudioSources=$INI_playerSettings__muteOtherAudioSources" \
"runInBackground=$INI_playerSettings__runInBackground" \
"stripEngineCode=$INI_playerSettings__stripEngineCode" \
"strippingLevel=$INI_playerSettings__strippingLevel" \
"androidIsGame=$INI_playerSettings__android_androidIsGame" \
"androidTVCompatibility=$INI_playerSettings__android_androidTVCompatibility" \
"blitType=$INI_playerSettings__android_blitType" \
"bundleVersionCode=$INI_playerSettings__android_bundleVersionCode" \
"disableDepthAndStencilBuffers=$INI_playerSettings__android_disableDepthAndStencilBuffers" \
"forceInternetPermission=$INI_playerSettings__android_forceInternetPermission" \
"forceSDCardPermission=$INI_playerSettings__android_forceSDCardPermission" \
"keystoreName=$INI_playerSettings__android_keystoreName" \
"keyaliasPass=$INI_playerSettings__android_keyaliasPass" \
"keyaliasName=$INI_playerSettings__android_keyaliasName" \
"keystorePass=$INI_playerSettings__android_keystorePass" \
"maxAspectRatio=$INI_playerSettings__android_maxAspectRatio" \
"minSdkVersion=$INI_playerSettings__android_minSdkVersion" \
"preferredInstallLocation=$INI_playerSettings__android_preferredInstallLocation" \
"showActivityIndicatorOnLoading=$INI_playerSettings__android_showActivityIndicatorOnLoading" \
"splashScreenScale=$INI_playerSettings__android_splashScreenScale" \
"targetDevice=$INI_playerSettings__android_targetDevice" \
"targetSdkVersion=$INI_playerSettings__android_targetSdkVersion" \
"useAPKExpansionFiles=$INI_playerSettings__android_useAPKExpansionFiles"
elif [ $platform == 'IOS']; then
$INI__executePath__unityPath \
-quit -batchmode \
-projectPath $programPath \
-logFile "$absoluteOutputPath/proj_program.log" \
-executeMethod AutoBuild.BuildPackage \
"version=$version" \
"platform=$platform" \
"outputPath=$relateOutputPath/package" \
"mode=$mode" \
"companyName=$INI_playerSettings__companyName" \
"productName=$INI_playerSettings__productName" \
"applicationIdentifier=$INI_playerSettings__applicationIdentifier" \
"colorSpace=$INI_playerSettings__colorSpace" \
"gpuSkinning=$INI_playerSettings__gpuSkinning" \
"graphicsJobs=$INI_playerSettings__graphicsJobs" \
"muteOtherAudioSources=$INI_playerSettings__muteOtherAudioSources" \
"runInBackground=$INI_playerSettings__runInBackground" \
"stripEngineCode=$INI_playerSettings__stripEngineCode" \
"strippingLevel=$INI_playerSettings__strippingLevel" \
"allowHTTPDownload=$INI_playerSettings__ios_allowHTTPDownload" \
"appInBackgroundBehavior=$INI_playerSettings__ios_appInBackgroundBehavior" \
"appleDeveloperTeamID=$INI_playerSettings__ios_appleDeveloperTeamID" \
"appleEnableAutomaticSigning=$INI_playerSettings__ios_appleEnableAutomaticSigning" \
"applicationDisplayName=$INI_playerSettings__ios_applicationDisplayName" \
"backgroundModes=$INI_playerSettings__ios_backgroundModes" \
"buildNumber=$INI_playerSettings__ios_buildNumber" \
"cameraUsageDescription=$INI_playerSettings__ios_cameraUsageDescription" \
"forceHardShadowsOnMetal=$INI_playerSettings__ios_forceHardShadowsOnMetal" \
"iOSManualProvisioningProfileID=$INI_playerSettings__ios_iOSManualProvisioningProfileID" \
"locationUsageDescription=$INI_playerSettings__ios_locationUsageDescription" \
"microphoneUsageDescription=$INI_playerSettings__ios_microphoneUsageDescription" \
"prerenderedIcon=$INI_playerSettings__ios_prerenderedIcon" \
"requiresFullScreen=$INI_playerSettings__ios_requiresFullScreen" \
"requiresPersistentWiFi=$INI_playerSettings__ios_requiresPersistentWiFi" \
"scriptCallOptimization=$INI_playerSettings__ios_scriptCallOptimization" \
"sdkVersion=$INI_playerSettings__ios_sdkVersion" \
"showActivityIndicatorOnLoading=$INI_playerSettings__ios_showActivityIndicatorOnLoading" \
"statusBarStyle=$INI_playerSettings__ios_statusBarStyle" \
"targetDevice=$INI_playerSettings__ios_targetDevice" \
"targetOSVersionString=$INI_playerSettings__ios_targetOSVersionString"

else
	echo "dont has this $platform only support Android and IOS"
fi
echo "end"


