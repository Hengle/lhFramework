using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
    using System.IO;

    public class ResourcesManager
    {
        public static ISource source { get; private set; }
        //private bool m
        private static ResourcesManager m_instance;
        public static ResourcesManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new ResourcesManager();
        }
        ResourcesManager()
        {
#if DEVELOPMENT
            source=new LocalSource();
#else
            source = new BundleSource();
#endif
        }
        public void Initialize()
        {
            source.Initialize();
        }
        public void Update()
        {
            source.Update();
        }
        public void LateUpdate()
        {
            source.LateUpdate();
        }
        public void Dispose()
        {
            source.Dispose();
            m_instance = null;
        }
        public static void LoadAsset(int assetId, DataHandler<UnityEngine.Object> onLoadOver,EVariantType variant=EVariantType.n)
        {
            source.Load(assetId, onLoadOver, variant);
        }
        public static void LoadAsset(int assetId, DataHandler<UnityEngine.Object[]> onLoadOver, EVariantType variant = EVariantType.n)
        {
            source.Load(assetId, onLoadOver, variant);
        }
        public static string LoadFile(string path)
        {
            var filePath = Define.sourceUrl + path;
            string file = null;
            using (FileStream fileStream=new FileStream(filePath,FileMode.Open,FileAccess.Read))
            {
                byte[] bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                file = System.Text.Encoding.UTF8.GetString(bytes);
            }
            return file;
        }
        public static byte[] LoadBytes(string path)
        {
            var filePath = Define.sourceUrl + path;
            byte[] bytes = null;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
            }
            return bytes;
        }
        public static FileStream LoadStream(string path)
        {
            var filePath = Define.sourceUrl + path;
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }
        public static void Destroy(int assetId,EVariantType variant=EVariantType.n)
        {
            source.Destroy(assetId);
        }
        public static void Unload(int assetId,EVariantType variant = EVariantType.n)
        {
            source.UnLoad(assetId, variant);
        }
        public static void UnloadUnusedAsset()
        {
            source.UnloadUnusedAsset();
        }
    }
}
