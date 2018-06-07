using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Core
{

    public class Define
    {
        public static readonly string platform =
#if UNITY_STANDALONE_WIN
        "StandaloneWindows64";
#elif UNITY_STANDALONE_OSX
        "StandloneOSXIntel";
#elif UNITY_ANDROID
            "Android";
#elif UNITY_IPHONE || UNITY_IOS
        "iOS";
#endif
        public static readonly EProjectType projectType =
#if RELEASE
        EProjectType.Release;
#elif DEVELOPMENT
        EProjectType.Development;
#elif DEBUG
        EProjectType.Debug;
#else
        EProjectType.Debug;
#endif
        public static readonly string streamingAssetUrl =
#if UNITY_ANDROID && !UNITY_EDITOR
        Application.streamingAssetsPath;
#else
    #if RELEASE
            "file://" + Application.streamingAssetsPath + "/";
    #else
            Application.streamingAssetsPath + "/";
#endif
#endif
        public static readonly string persistentUrl = Application.persistentDataPath + "/" ;

        public static readonly string localSourceUrl= Application.dataPath + "/Arts/";
        public static readonly string dataUrl = Application.dataPath + "/Resources/";
        public static readonly string tempUrl = Application.persistentDataPath + "/Temp/";
        public static readonly string sourceUrl =
#if RELEASE
        persistentUrl+ platform + "/"+Const.bundleFolder+"/";
#elif DEBUG
        streamingAssetUrl + platform + "/"+ Const.bundleFolder + "/";
#elif DEVELOPMENT
        localSourceUrl ;
#else
        streamingAssetUrl+ platform + "/"+Const.bundleFolder+"/";
#endif
        public static readonly string sourceTableUrl =

#if DEVELOPMENT
            streamingAssetUrl + platform + "/" + Const.bundleFolder + "/"
#else
            sourceUrl
#endif
            + Const.souceTableName;


    }
}
