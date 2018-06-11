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
"mode=$mode"

elif [ $platform == 'IOS']; then
$INI__executePath__unityPath \
-quit -batchmode \
-projectPath $programPath \
-logFile "$absoluteOutputPath/proj_program.log" \
-executeMethod AutoBuild.BuildPackage "$version" "$platform" "$relateOutputPath/package" "$mode"

else
echo "dont has this $platform"
fi
echo "end"


