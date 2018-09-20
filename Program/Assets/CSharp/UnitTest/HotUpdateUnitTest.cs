using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    using Infrastructure.Managers;
    using Infrastructure.Core;
    public class HotUpdateUnitTest : MonoBehaviour
    {
#if RELEASE
        HotUpdateManager m_manager;
#endif
        // Use this for initialization
        void Start()
        {
#if RELEASE
            m_manager = HotUpdateManager.GetInstance();
            m_manager.downloadCompletedHandler = () =>
            {
                UnityEngine.Debug.Log("success");
                m_manager.CombineAsset();
            };
            m_manager.needDownloadHandler = (length) =>
            {
                UnityEngine.Debug.Log(length);
                m_manager.Download();
            };
            m_manager.uncompressProgressHandler = (progress) =>
            {
                UnityEngine.Debug.Log(progress);
            };
            m_manager.downloadProgressHandler = (progress) =>
            {
                UnityEngine.Debug.Log(progress);
            };
            m_manager.remoteVersionNetworkErrorHandler = (err) =>
            {
                UnityEngine.Debug.LogError(err);
            };
            m_manager.remoteChangeInfoNetworkErrorHandler = (err) =>
            {
                UnityEngine.Debug.LogError(err);
            };
            m_manager.remoteDownloadNetworkErrorHandler = () =>
            {
                UnityEngine.Debug.LogError("download error");
            };
            m_manager.remoteRootInfoNetworkErrorHandler = (err) =>
            {
                UnityEngine.Debug.LogError(err);
            };
            m_manager.combineProgressHandler = (progress) =>
            {
                UnityEngine.Debug.Log(progress);
            };
            m_manager.combineCompleteHandler = () =>
            {
                UnityEngine.Debug.Log("combine success");
            };
            m_manager.CheckVersion();
#endif
        }

        // Update is called once per frame
        void Update()
        {
#if RELEASE
            m_manager.Update();
#endif
        }
        void OnDestroy()
        {
#if RELEASE
            m_manager.Dispose();
#endif
        }
    }
}