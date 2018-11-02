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
            Application.streamingAssetsPath + "/";//"file://" + 
#else
            Application.streamingAssetsPath + "/";
#endif
#endif
        public static readonly string persistentUrl = Application.persistentDataPath + "/" ;

        public static readonly string localSourceUrl= Application.dataPath + "/Arts/";
        public static readonly string dataUrl = Application.dataPath + "/Resources/";
        public static readonly string tempUrl = Application.persistentDataPath + "/Temp/";
        public static string host = "http://172.25.53.179/lhFramework";//"http://172.25.51.159:8585/HFS/lhFramework";

        public static readonly string sourceUrl =
#if RELEASE
        persistentUrl + platform + "/";
#elif DEBUG
        streamingAssetUrl + platform + "/";
#elif DEVELOPMENT
        localSourceUrl ;
#else
        streamingAssetUrl+ platform + "/";
#endif
        public static readonly string sourceBundleUrl = sourceUrl
#if DEVELOPMENT
        ;
#else
        +Const.sourceBundleFolder+"/";
#endif
        public static readonly string sceneBundleUrl = sourceUrl
#if DEVELOPMENT
        ;
#else
        +Const.sceneBundleFolder+"/";
#endif
        public static readonly string sourceTableUrl =

#if DEVELOPMENT
            streamingAssetUrl + platform + "/" + Const.sourceBundleFolder + "/"
#else
            sourceBundleUrl
#endif
            + Const.sourceTableName;
        public static readonly string sceneTableUrl =
#if DEVELOPMENT
            streamingAssetUrl + platform + "/" + Const.sceneBundleFolder + "/"
#else
            sceneBundleUrl
#endif
            + Const.sceneTableName;

    }
}
