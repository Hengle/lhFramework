#!/bin/sh
echo "start"

#unityapp的路径
export unity=/Applications/Unity/Unity.app/Contents/MacOS/Unity

rootPath=/Users/laohan/workSpace/Unity/lhFramework
#这里就可以拿到jenkins传递进来的参数了
#把所有=后面的参数取出来
for a in $*
do
    r=`echo $a | sed "s/--//g"`
    eval $r
done

dirname="pack-$platform-"$(date +%Y_%m_%d_%H_%M)
absoluteOutputPath=$rootPath/AutoBuild/Output/$platform/$dirname
relateOutputPath=../AutoBuild/Output/$platform/$dirname
#运行时项目路径
programPath=$rootPath/Program
#编辑器项目路径
artPath=$rootPath/Art/$platform
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
$unity -quit -batchmode -projectPath $artPath -logFile "$absoluteOutputPath/proj_art.log" -executeMethod AutoBuild.BuildPackage "$editor_maintainer_filepath" "$mode"
echo "package-------------------->  ui"

echo "package-------------------->  program"
#程序打包  unity产生log就写在tmp/1.log里面，比如Debug.Log和Unity编辑器产生的。
$unity -quit -batchmode -projectPath $programPath -logFile "$absoluteOutputPath/proj_program.log"  -executeMethod AutoBuild.BuildPackage "$version" "$platform" "$relateOutputPath/package" "$mode"

echo "end"

read flag