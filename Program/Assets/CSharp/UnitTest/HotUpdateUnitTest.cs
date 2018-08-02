using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    using Infrastructure.Managers;
    using Infrastructure.Core;
    public class HotUpdateUnitTest : MonoBehaviour
    {
        HotUpdateManager m_manager;
        // Use this for initialization
        void Start()
        {
            var s = Define.streamingAssetUrl;
            m_manager = HotUpdateManager.GetInstance();
            m_manager.Check();
        }

        // Update is called once per frame
        void Update()
        {
            m_manager.Update();
        }
        void OnDestroy()
        {
            m_manager.Dispose();
        }
    }
}