using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Infrastructure
{
    public class Infrastructure : MonoBehaviour
    {
        private ResourcesManager m_resourcesManager;
        void Start()
        {
            m_resourcesManager = ResourcesManager.GetInstance();
        }
        void Update()
        {
            m_resourcesManager.Update();
        }
        void OnDestroy()
        {
            m_resourcesManager.Dispose();
        }
    }
}