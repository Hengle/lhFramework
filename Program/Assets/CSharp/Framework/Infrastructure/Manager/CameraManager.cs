using Cinemachine;
using System.Collections;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    using Core;
    public class CameraManager
    {
        public static Camera mainCamera
        {
            get
            {
                return m_instance.m_mainCamera;
            }
        }
        public static CinemachineBrain cinemachineBrain
        {
            get
            {
                return m_instance.m_cinemachineBrain;
            }
        }
        private GameObject m_cameraObj;
        private Camera m_mainCamera;
        private CinemachineBrain m_cinemachineBrain;
        private static CameraManager m_instance;
        public static CameraManager GetInstance()
        {
            if (m_instance != null) return null;
            return m_instance = new CameraManager();
        }
        CameraManager()
        {

        }
        ~CameraManager()
        {

        }
        public void Initialize()
        {
            m_cameraObj = new GameObject("Main Camera");
            m_mainCamera = m_cameraObj.AddComponent<Camera>();
            m_cinemachineBrain =m_cameraObj.AddComponent<Cinemachine.CinemachineBrain>();

            m_cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            m_cinemachineBrain.m_DefaultBlend.m_Time =0.3f;

            m_cameraObj.transform.position = Vector3.zero;
            m_cameraObj.transform.eulerAngles = Vector3.zero;
            m_mainCamera.clearFlags = CameraClearFlags.SolidColor;
            //m_mainCamera.cullingMask = (1 << LayerMask.NameToLayer(Layer.char_) | 1 << LayerMask.NameToLayer(Layer.char_shadow) | 1 << LayerMask.NameToLayer(Layer.char_weapon) | 1 << LayerMask.NameToLayer(Layer.wall) | 1 << LayerMask.NameToLayer(Layer.t4m) | 1 << LayerMask.NameToLayer(Layer.defaultLayer));
            m_mainCamera.farClipPlane = 1000;
            m_mainCamera.fieldOfView = 50;
            m_mainCamera.allowMSAA = false;
            m_mainCamera.allowHDR = false;
            //if (Macro.qualityType!=EQualityType.High)
            //{
            //    External.ExternalManager.CameraSet(m_mainCamera);
            //}
        }
        public void Update()
        {
            //RaycastHit hitInfo;
            //Ray ray=new Ray(m_mainCamera.transform.position,m_mainCamera.transform.forward);
            //UnityEngine.Debug.DrawRay(m_mainCamera.transform.position, m_mainCamera.transform.forward*1000);

            //if (Physics.Raycast(ray, out hitInfo, 1000, -1))
            //{
            //    Debug.Log.i(Debug.ELogType.Info, hitInfo.distance.ToString());
            //}
#if UNITY_EDITOR
            if (Macro.overdraw)
            {
                if (Application.isPlaying)
                {
                    var view = UnityEditor.SceneView.lastActiveSceneView;
                    if (view == null) return;
                    view.orthographic = false;
                    view.LookAtDirect(m_cameraObj.transform.position, m_cameraObj.transform.rotation, 0.001f);
                }
            }
#endif
        }

        public void ShowCamera(bool show)
        {
            m_cameraObj.SetActive(show);
        }
        public void Dispose()
        {
            UnityEngine.Object.Destroy(m_cameraObj);
            m_instance = null;
        }
    }
}
