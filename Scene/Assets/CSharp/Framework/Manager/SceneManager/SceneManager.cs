using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.Infrastructure.Managers
{
    using System;
    using System.Threading.Tasks;
    using Core;
    public class SceneManager :Singleton<SceneManager>
    {
        private IScene m_scene;
        public override async Task Initialize(Action onInitialOver = null)
        {
#if DEVELOPMENT
            m_scene=new LocalScene();
#else
            m_scene = new BundleScene();
#endif
            m_scene.Initialize();
        }
        public void Load()
        {
            m_scene.Load();
        }
        public void UnLoad()
        {
            m_scene.Unload();
        }
        public override void Dispose()
        {
            m_scene.Dispose();
            m_scene = null;
            base.Dispose();
        }
    }
}