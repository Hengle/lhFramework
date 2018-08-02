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

if [[ -z $platform || -z $mode || -z $versionType ]];
then
echo -e "error:must has platform mode versionType\nplatform====>13:Android  9:iOS  19:StandaloneWindows64  24:StandaloneLinux64\nmode===>debug  release\nversionType====>base,alpha,beta,RC,release\nversionType=====>base,alpha,beta,RC,release"
exit 0
fi

if [ $platform == 'Android' ];then
platform=13
elif [ $platform == "iOS" ];then
platform=9
elif [ $platform == 'Windows' ];then
platform=5
elif [ $platform == 'MacOSX' ];then
platform=27
else
echo "dont support this platform:"$platform
fi
cd `dirname $0`
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
revised=$(($revised+1))
echo "dont has this versionType:"$versionType

dirname="source-$platform-"$(date +%Y_%m_%d_%H_%M)
date=$(date +%Y%m%d)
version=$major.$minor.$revised.$date.$versionType
absoluteOutputPath=$INI__executePath__packageAbsoluteOutput/$platform/$dirname
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

echo "output ------------------------------------------------------------>  all variables"
echo "platform = $platform"
echo "version = $version"
echo "mode = $mode"
echo "artPath = $artPath"
echo "absoluteOutputPath = $absoluteOutputPath"
echo "packageRelateOutputPath = $INI__executePath__packageRelateOutputPath/$platform/$dirname/package"
echo "dirname = $dirname"

echo "package------------------------------------------------------------>  art"
#美术资源监测。  打开unity3d  执行AutoBuild.Build 方法。
#$INI__executePath__unityPath \
#-quit -batchmode \
#-projectPath $artPath \
#-logFile "$absoluteOutputPath/proj_art.log" \
#-executeMethod AutoBuild.BuildSource \
#"maintainer_filePath=$editor_maintainer_filepath" \
#"mode=$mode" \
#"platform=$platform" \
#"rootName=$INI__art__rootName" \
#"currentLevel=$INI__qualitySettings__currentLevel"
echo outputLog=="file://$absoluteOutputPath/proj_art.log"
echo "package------------------------------------------------------------>  ui"

echo "svn----------------------------------------------------------------> commit to bundle "
if test svn
then
echo "has svn"
svn commit -m "commit bundle sources for art"
fi

echo "save---------------------------------------------------------------> version to local"
(
cat << EOF
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
cat << EOF
$version
EOF
) > $artPath/Assets/StreamingAssets/$platform/version
echo "copy---------------------------------------------------------------> to target folder"
sourcePlatformFolder=$INI__executePath__sourcePath/$platform
if [ ! -d $sourcePlatformFolder ];then
mkdir $sourcePlatformFolder
fi
majorFolder=$sourcePlatformFolder/${major}
if [ ! -d $majorFolder ];then
mkdir $majorFolder
fi
sourcePath=$majorFolder/${major}_${minor}_${revised}_$versionType
mkdir $sourcePath
rsync -avP --exclude=*.meta --exclude=*.manifest --exclude=EditorSourceTable.txt $artPath/Assets/StreamingAssets/$platform/ $sourcePath >/dev/null
cp -a $artPath/Assets/StreamingAssets/$platform/version $majorFolder/version
if test svn
then
echo "has svn"
svn add *
svn commit -m "commit bundle sources for art"
fi
echo "package------------------------------------------------------------>  program"
echo "end"
