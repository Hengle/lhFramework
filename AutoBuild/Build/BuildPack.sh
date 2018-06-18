#!/bin/sh
echo "start"
#这里就可以拿到jenkins传递进来的参数了
#platform====>13:Android  9:iOS  19:StandaloneWindows64  24:StandaloneLinux64
#把所有=后面的参数取出来
if test svn
then
echo "has svn"
svn update
svn revert
fi
for a in $*
do
r=`echo $a | sed "s/--//g"`
eval $r
done

if [[ -z $platform || -z $versionType || -z $versionMode || -z $mode ]];
then
echo -e "error:must has platform versionType versionMode mode\nplatform====>13:Android  9:iOS  19:StandaloneWindows64  24:StandaloneLinux64\nmode===>debug  release\nversionType====>base,alpha,beta,RC,release\nversionMode=====>1: major  2:minor  3:revised"
exit 0
fi

. ./read_ini.sh

read_ini ../Config/properties.ini
if [ $mode == "debug" ];then
	read_ini ../Config/properties_debug.ini
elif [ $mode == "release" ];then
	read_ini ../Config/properties_release.ini
else
	echo "dont has this mode:"$mode
fi

read_ini ../Config/version.ini
major=$INI__version__major
minor=$INI__version__minor
revised=$INI__version__revised
if [ $versionType == 'major' ]; then
major=$(($major+1))
minor=0
revised=0
elif [ $versionType == 'minor' ];then
minor=$(($minor+1))
revised=0
elif [ $versionType == 'revised' ];then
revised=$(($revised+1))
else
echo "dont has this versionType:"$versionType
fi

dirname="pack-$platform-"$(date +%Y_%m_%d_%H_%M)
date=$(date +%Y%m%d)
version=$major.$minor.$revised.$date.$versionMode
absoluteOutputPath=$INI__executePath__pacageAbsoluteOutput/$platform/$dirname
#运行时程序项目路径
programPath=$INI__executePath__rootPath/Program
#编辑器资源项目路径
artPath=$INI__executePath__rootPath/Art
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

echo "output -------------------->  all variables"
echo "version = $version"
echo "platform = $platform"
echo "mode = $mode"
echo "programPath = $programPath"
echo "artPath = $artPath"
echo "absoluteOutputPath = $absoluteOutputPath"
echo "packageRelateOutputPath = $INI__executePath__packageRelateOutputPath/$platform/$dirname/package"
echo "dirname = $dirname"

echo "package-------------------->  art"
#美术资源监测。  打开unity3d  执行AutoBuild.Build 方法。
$INI__executePath__unityPath \
-quit -batchmode \
-projectPath $artPath \
-logFile "$absoluteOutputPath/proj_art.log" \
-executeMethod AutoBuild.BuildSource \
"maintainer_filePath=$editor_maintainer_filepath" \
"mode=$mode" \
"platform=$platform" \
"currentLevel=$INI__qualitySettings__currentLevel"

echo "package-------------------->  ui"

echo "svn------------------------> commit to bundle "
if test svn
then
echo "has svn"
svn commit -m "commit bundle sources for art"
fi

echo "copy-----------------------> to target folder"
cp -p $artPath/Assets/StreamingAssets/$platform/* $INI__executePath__sourcePath/dirname/
if test svn
then
echo "has svn"
svn add *
svn commit -m "commit bundle sources for art"
fi
echo "save-----------------------> version to local"
(
cat <<EOF
[version]
; 主版本号
major=$major
; 次版本号
minor=$minor
; 修订版本号
revised=$revised
EOF
) > ../Config/version.ini

(
cat <<EOF
$version
EOF
) > $artPath/Assets/StreamingAssets/$platform/version
echo "package-------------------->  program"
if test svn
then
echo "has svn"
svn update
svn revert
fi
#程序打包  unity产生log就写在tmp/1.log里面，比如Debug.Log和Unity编辑器产生的。
if [ $platform == 13 ]; then
$INI__executePath__unityPath \
-quit -batchmode \
-projectPath $programPath \
-logFile "$absoluteOutputPath/proj_program.log" \
-executeMethod AutoBuild.BuildPackage \
"version=$version" \
"platform=$platform" \
"outputPath=$INI__executePath__packageRelateOutputPath/$platform/$dirname/package" \
"mode=$mode" \
"companyName=$INI__playerSettings__companyName" \
"productName=$INI__playerSettings__productName" \
"applicationIdentifier=$INI__playerSettings__applicationIdentifier" \
"colorSpace=$INI__playerSettings__colorSpace" \
"gpuSkinning=$INI__playerSettings__gpuSkinning" \
"graphicsJobs=$INI__playerSettings__graphicsJobs" \
"muteOtherAudioSources=$INI__playerSettings__muteOtherAudioSources" \
"runInBackground=$INI__playerSettings__runInBackground" \
"stripEngineCode=$INI__playerSettings__stripEngineCode" \
"strippingLevel=$INI__playerSettings__strippingLevel" \
"androidIsGame=$INI__playerSettings__android_androidIsGame" \
"androidTVCompatibility=$INI__playerSettings__android_androidTVCompatibility" \
"blitType=$INI__playerSettings__android_blitType" \
"bundleVersionCode=$INI__playerSettings__android_bundleVersionCode" \
"disableDepthAndStencilBuffers=$INI__playerSettings__android_disableDepthAndStencilBuffers" \
"forceInternetPermission=$INI__playerSettings__android_forceInternetPermission" \
"forceSDCardPermission=$INI__playerSettings__android_forceSDCardPermission" \
"keystoreName=$INI__playerSettings__android_keystoreName" \
"keyaliasPass=$INI__playerSettings__android_keyaliasPass" \
"keyaliasName=$INI__playerSettings__android_keyaliasName" \
"keystorePass=$INI__playerSettings__android_keystorePass" \
"maxAspectRatio=$INI__playerSettings__android_maxAspectRatio" \
"minSdkVersion=$INI__playerSettings__android_minSdkVersion" \
"preferredInstallLocation=$INI__playerSettings__android_preferredInstallLocation" \
"showActivityIndicatorOnLoading=$INI__playerSettings__android_showActivityIndicatorOnLoading" \
"splashScreenScale=$INI__playerSettings__android_splashScreenScale" \
"targetDevice=$INI__playerSettings__android_targetDevice" \
"targetSdkVersion=$INI__playerSettings__android_targetSdkVersion" \
"useAPKExpansionFiles=$INI__playerSettings__android_useAPKExpansionFiles" \
"development=$INI__buildSettings__development" \
"connectProfiler=$INI__buildSettings__connectProfiler" \
"buildScriptsOnly=$INI__buildSettings__buildScriptsOnly" \
"allowDebugging=$INI__buildSettings__allowDebugging" \
"compressFilesInPackage=$INI__buildSettings__compressFilesInPackage" \
"compressWithPsArc=$INI__buildSettings__compressWithPsArc" \
"enableHeadlessMode=$INI__buildSettings__enableHeadlessMode" \
"explicitDivideByZeroChecks=$INI__buildSettings__explicitDivideByZeroChecks" \
"explicitNullChecks=$INI__buildSettings__explicitNullChecks" \
"androidBuildSystem=$INI__buildSettings__android_androidBuildSystem" \
"androidBuildSubtarget=$INI__buildSettings__android_androidBuildSubtarget" \
"androidDebugMinification=$INI__buildSettings__android_androidDebugMinification" \
"androidReleaseMinification=$INI__buildSettings__android_androidReleaseMinification" \
"androidDeviceSocketAddress=$INI__buildSettings__android_androidDeviceSocketAddress" \
"iOSBuildConfigType=$INI__buildSettings__ios_iOSBuildConfigType" \
"currentLevel=$INI__qualitySettings__currentLevel"
elif [ $platform == 9 ]; then
$INI__executePath__unityPath \
-quit -batchmode \
-projectPath $programPath \
-logFile "$absoluteOutputPath/proj_program.log" \
-executeMethod AutoBuild.BuildPackage \
"version=$version" \
"platform=$platform" \
"outputPath=$INI__executePath__packageRelateOutputPath/$platform/$dirname/package" \
"mode=$mode" \
"companyName=$INI__playerSettings__companyName" \
"productName=$INI__playerSettings__productName" \
"applicationIdentifier=$INI__playerSettings__applicationIdentifier" \
"colorSpace=$INI__playerSettings__colorSpace" \
"gpuSkinning=$INI__playerSettings__gpuSkinning" \
"graphicsJobs=$INI__playerSettings__graphicsJobs" \
"muteOtherAudioSources=$INI__playerSettings__muteOtherAudioSources" \
"runInBackground=$INI__playerSettings__runInBackground" \
"stripEngineCode=$INI__playerSettings__stripEngineCode" \
"strippingLevel=$INI__playerSettings__strippingLevel" \
"allowHTTPDownload=$INI__playerSettings__ios_allowHTTPDownload" \
"appInBackgroundBehavior=$INI__playerSettings__ios_appInBackgroundBehavior" \
"appleDeveloperTeamID=$INI__playerSettings__ios_appleDeveloperTeamID" \
"appleEnableAutomaticSigning=$INI__playerSettings__ios_appleEnableAutomaticSigning" \
"applicationDisplayName=$INI__playerSettings__ios_applicationDisplayName" \
"backgroundModes=$INI__playerSettings__ios_backgroundModes" \
"buildNumber=$INI__playerSettings__ios_buildNumber" \
"cameraUsageDescription=$INI__playerSettings__ios_cameraUsageDescription" \
"forceHardShadowsOnMetal=$INI__playerSettings__ios_forceHardShadowsOnMetal" \
"iOSManualProvisioningProfileID=$INI__playerSettings__ios_iOSManualProvisioningProfileID" \
"locationUsageDescription=$INI__playerSettings__ios_locationUsageDescription" \
"microphoneUsageDescription=$INI__playerSettings__ios_microphoneUsageDescription" \
"prerenderedIcon=$INI__playerSettings__ios_prerenderedIcon" \
"requiresFullScreen=$INI__playerSettings__ios_requiresFullScreen" \
"requiresPersistentWiFi=$INI__playerSettings__ios_requiresPersistentWiFi" \
"scriptCallOptimization=$INI__playerSettings__ios_scriptCallOptimization" \
"sdkVersion=$INI__playerSettings__ios_sdkVersion" \
"showActivityIndicatorOnLoading=$INI__playerSettings__ios_showActivityIndicatorOnLoading" \
"statusBarStyle=$INI__playerSettings__ios_statusBarStyle" \
"targetDevice=$INI__playerSettings__ios_targetDevice" \
"targetOSVersionString=$INI__playerSettings__ios_targetOSVersionString" \
"development=$INI__buildSettings__development" \
"connectProfiler=$INI__buildSettings__connectProfiler" \
"buildScriptsOnly=$INI__buildSettings__buildScriptsOnly" \
"allowDebugging=$INI__buildSettings__allowDebugging" \
"compressFilesInPackage=$INI__buildSettings__compressFilesInPackage" \
"compressWithPsArc=$INI__buildSettings__compressWithPsArc" \
"enableHeadlessMode=$INI__buildSettings__enableHeadlessMode" \
"explicitDivideByZeroChecks=$INI__buildSettings__explicitDivideByZeroChecks" \
"explicitNullChecks=$INI__buildSettings__explicitNullChecks" \
"androidBuildSystem=$INI__buildSettings__android_androidBuildSystem" \
"androidBuildSubtarget=$INI__buildSettings__android_androidBuildSubtarget" \
"androidDebugMinification=$INI__buildSettings__android_androidDebugMinification" \
"androidReleaseMinification=$INI__buildSettings__android_androidReleaseMinification" \
"androidDeviceSocketAddress=$INI__buildSettings__android_androidDeviceSocketAddress" \
"iOSBuildConfigType=$INI__buildSettings__ios_iOSBuildConfigType" \
"currentLevel=$INI__qualitySettings__currentLevel"

xcodebuild \
clean build \
-quiet \
-version \
-project $absoluteOutputPath/package/Unity-iPhone.xcodeproj \
-scheme Unity-iPhone \
-configuration $mode \
archive -archivePath $absoluteOutputPath/package/app.xcarchive


xcodebuild \
-quiet \
-version \
-exportArchive \
-archivePath $absoluteOutputPath/package.xcarchive \
-exportPath $absoluteOutputPath/package/build/$mode-iphoneos/ \
-allowProvisioningUpdates
# -exportOptionsPlist $plist

else
	echo "dont has this $platform only support Android and IOS"
fi
echo "end"


