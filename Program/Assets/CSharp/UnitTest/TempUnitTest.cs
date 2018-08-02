using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace lhFramework.UnitTest
{
    public class TempUnitTest:MonoBehaviour
    {
        void Start()
        {
            byte[] bytes = new byte[25] {56,46,48,46,55,48,46,50,48,49,56,48,55,48,48,46,97,108,112,104,97,0,0,0,0};
            string s = System.Text.Encoding.UTF8.GetString(bytes);
            s = s + "tttt";
            UnityEngine.Debug.Log(s);
        }
        
        void Update()
        {

        }
        void OnGUI()
        {
        }
    }
}
