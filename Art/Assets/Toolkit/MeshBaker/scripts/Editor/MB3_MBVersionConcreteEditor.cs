/**
 *	\brief Hax!  DLLs cannot interpret preprocessor directives, so this class acts as a "bridge"
 */
using System;
using UnityEngine;
using UnityEditor;
using System.Collections;

namespace DigitalOpus.MB.Core{

	public class MBVersionEditorConcrete:MBVersionEditorInterface{
		//Used to map the activeBuildTarget to a string argument needed by TextureImporter.GetPlatformTextureSettings
		//The allowed values for GetPlatformTextureSettings are "Web", "Standalone", "iPhone", "Android" and "FlashPlayer".
		public string GetPlatformString(){
			#if (UNITY_4_6 || UNITY_4_7 || UNITY_4_5 || UNITY_4_3 || UNITY_4_2 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0 || UNITY_3_5)
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone){
				return "iPhone";	
			}
			#else
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS){
				return "iPhone";	
			}
			#endif
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer)
            {
                return "Windows Store Apps";
            }
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.PSP2)
            {
                return "PSP2";
            }
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.PS4)
            {
                return "PS4";
            }
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.XboxOne)
            {
                return "XboxOne";
            }
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.SamsungTV)
            {
                return "Samsung TV";
            }
#if (UNITY_5_5_OR_NEWER)
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.N3DS)
            {
                return "Nintendo 3DS";
            }
#endif
#if (UNITY_5_3 || UNITY_5_2 || UNITY_5_3_OR_NEWER)
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WiiU)
            {
                return "WiiU";
            }
#endif
#if (UNITY_5_3 || UNITY_5_3_OR_NEWER)
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS)
            {
                return "tvOS";
            }
#endif
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Tizen)
            {
                return "Tizen";
            }
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android){
				return "Android";
			}
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinux64 ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneLinuxUniversal ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows ||
			    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 ||
			    EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel ||
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel64 ||
#if UNITY_2017_3_OR_NEWER
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSX
#else
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXUniversal
#endif
                )
            {
				return "Standalone";	
			}
#if !UNITY_5_4_OR_NEWER
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayer ||
			    EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayerStreamed
                )
            {
				return "Web";
			}
#endif
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                return "WebGL";
            }
            return null;
		}

//		public int GetMaximumAtlasDimension(){
//			int atlasMaxDimension = 4096;
//			if (!Application.isPlaying){		
//				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) atlasMaxDimension = 2048;
//				#if (UNITY_4_6 || UNITY_4_7 || UNITY_4_5 || UNITY_4_3 || UNITY_4_2 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0 || UNITY_3_5)
//				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone) atlasMaxDimension = 4096;
//				#else
//				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS) atlasMaxDimension = 4096;
//				#endif
//				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayer) atlasMaxDimension = 2048;
//				if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebPlayerStreamed) atlasMaxDimension = 2048;
//			} else {			
//				if (Application.platform == RuntimePlatform.Android) atlasMaxDimension = 2048;
//				if (Application.platform == RuntimePlatform.IPhonePlayer) atlasMaxDimension = 4096;
//				if (Application.platform == RuntimePlatform.WindowsWebPlayer) atlasMaxDimension = 2048;
//				if (Application.platform == RuntimePlatform.OSXWebPlayer) atlasMaxDimension = 2048;
//			}
//			return atlasMaxDimension;
//		}

		public void RegisterUndo(UnityEngine.Object o, string s){
#if (UNITY_4_2 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0 || UNITY_3_5)
			Undo.RegisterUndo(o, s);
#else
			Undo.RecordObject(o,s);
#endif
		}
		
		public void SetInspectorLabelWidth(float width){
#if (UNITY_4_2 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0 || UNITY_3_5)
			EditorGUIUtility.LookLikeControls(width);
#else
			EditorGUIUtility.labelWidth = width;
#endif
		}
	}
}