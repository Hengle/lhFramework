using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UnitTest
{
    using Infrastructure;
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
            ((BundleSource)ResourcesManager.source).LoadAll(bundleCount);
        }
    }
}
