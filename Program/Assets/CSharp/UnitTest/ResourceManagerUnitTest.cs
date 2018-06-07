using System;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    using lhFramework.Infrastructure.Managers;
    public class ResourceManagerUnitTest:MonoBehaviour
    {
        public int bundleCount;
        private ResourcesManager m_resources;
        void Start()
        {
            m_resources = ResourcesManager.GetInstance();
            m_resources.Initialize();
            Invoke("LoadAll", 1);
        }
        void Update()
        {
            m_resources.Update();
        }
        void LateUpdate()
        {
            m_resources.LateUpdate();
        }
        void OnDestroy()
        {
            m_resources.Dispose();
        }
        void LoadAll()
        {
            //int[] assetIds = new int[] { 120000, 100000 , 100001 , 100002 , 100003 };
            //for (int i = 0; i < assetIds.Length; i++)
            //{
            //    ResourcesManager.Load(assetIds[i], delegate(UnityEngine.Object o){
            //        UnityEngine.Debug.Log(o.name);
            //    });
            //}
            ((BundleSource)ResourcesManager.source).LoadAll(bundleCount);
        }
    }
}
