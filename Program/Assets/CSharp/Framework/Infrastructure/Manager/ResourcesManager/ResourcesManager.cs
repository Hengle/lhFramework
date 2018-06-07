using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
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
        public static void Load(int assetId, DataHandler<UnityEngine.Object> onLoadOver,EVariantType variant=EVariantType.n)
        {
            source.Load(assetId, onLoadOver, variant);
        }
        public static void Load(int assetId, DataHandler<UnityEngine.Object[]> onLoadOver, EVariantType variant = EVariantType.n)
        {
            source.Load(assetId, onLoadOver, variant);
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
