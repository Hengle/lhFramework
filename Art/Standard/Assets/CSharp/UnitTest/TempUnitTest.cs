using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UnitTest
{
    public class TempUnitTest:MonoBehaviour
    {
        public string path;
        void Start()
        {
        }
        void Update()
        {

        }
        void OnGUI()
        {
            if (GUILayout.Button("Load"))
            {
#if UNITY_EDITOR
                var s = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath("Assets/"+ path);
                UnityEngine.Debug.Log(s.Length);
#endif
            }
        }
    }
}
