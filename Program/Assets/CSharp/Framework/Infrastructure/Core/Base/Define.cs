using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Core
{

    public class Define
    {
        public static readonly int platform =
#if UNITY_STANDALONE_WIN
        5;
#elif UNITY_STANDALONE_OSX
        27;
#elif UNITY_ANDROID
        13;
#elif UNITY_IPHONE || UNITY_IOS
        9;
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
        public static string host = "http://172.25.54.49/lhFramework";//"http://172.25.51.159:8585/HFS/lhFramework";
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
