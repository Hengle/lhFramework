using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    public class ShaderManager
    {
        private static ShaderManager m_instance;
        private Dictionary<string, Shader> m_dic = new Dictionary<string, Shader>();
        public static ShaderManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new ShaderManager();
        }
        ShaderManager()
        {
                
        }
        public static void Store(Shader[] shaders)
        {
            for (int i = 0; i < shaders.Length; i++)
            {
                m_instance.m_dic.Add(shaders[i].name, shaders[i]);
            }
        }
        public static Shader Get(string name)
        {
#if UNITY_EDITOR
            if (m_instance.m_dic.ContainsKey(name))
            {
                return m_instance.m_dic[name];
            }
            else
            {
                Debug.Log.i(Debug.ELogType.Error, "dont has this shader: " + name);
            }
#endif
            return m_instance.m_dic[name];
        }
    }
}
