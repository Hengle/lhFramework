using System;
using System.Collections.Generic;
using UnityEngine;
namespace Framework.Infrastructure
{
    public class MaterialManager
    {
        private Dictionary<int, Material> m_dic = new Dictionary<int, Material>();
        private static MaterialManager m_instance;
        public static MaterialManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new MaterialManager();
        }
        MaterialManager()
        {
        }
        public void Initialize(Action onInitialOver)
        {
            //var materialDic = Config.ConfigManager.sBaseConfig.material;
            //int count = materialDic.Count;
            //if (count<=0)
            //{
            //    onInitialOver();
            //    return;
            //}
            //foreach (var item in materialDic)
            //{
            //    var value = item.Value;
            //    var key = item.Key;
            //    var type = Enum.Parse(typeof(EMaterialType), key);
            //    External.ExternalManager.LoadAsset(External.EAssetType.Shader,value, (o) =>
            //    {
            //        count--;
            //        if (o==null)
            //        {
            //            if (count <= 0)
            //            {
            //                onInitialOver();
            //            }
            //        }
            //        else
            //        {
            //            var s = o as Shader;
            //            if (s.isSupported && SystemInfo.supportsImageEffects)
            //            {
            //                m_dic.Add((int)type, new Material(s));
            //            }
            //            if (count <= 0)
            //            {
            //                onInitialOver();
            //            }
            //        }
            //    });
            //}
        }
        public void Dispose()
        {
            //foreach (var item in m_dic)
            //{
            //    External.ExternalManager.UnLoadAsset(External.EAssetType.Shader, item.Value, "");
            //}
            m_instance = null;
        }
        public static Material Get(EMaterialType type)
        {
            if (m_instance.m_dic.ContainsKey((int)type))
            {
                return m_instance.m_dic[(int)type];
            }
            return null;
        }
    }
}
