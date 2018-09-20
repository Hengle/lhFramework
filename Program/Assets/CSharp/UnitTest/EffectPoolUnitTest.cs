using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    using System;
    using Infrastructure.Managers;
    using Infrastructure.Core;

    public class EffectPoolUnitTest : MonoBehaviour
    {
        public List<GameObject> list = new List<GameObject>();
        public List<GameObject> listDelay = new List<GameObject>();
        EffectManager m_effectManager;
        TimeManager m_timeManager;
        private List<IEffect> usedTest = new List<IEffect>();
        private List<IEffect> usedDelayTest = new List<IEffect>();
        // Use this for initialization
        void Start()
        {
            m_timeManager = TimeManager.GetInstance();
            m_effectManager = EffectManager.GetInstance();
            m_effectManager.loadHandler = OnLoad;
            m_effectManager.destroyHandler = ONDestroy;
            Store(()=> {
                InvokeRepeating("RandomAdd", 0.2f, 0.2f);
                InvokeRepeating("RandomAddDelay", 1.2f, 0.8f);
                InvokeRepeating("RandomRemove", 2f, 1.3f);
            });
        }

        // Update is called once per frame
        void Update()
        {
            m_timeManager.Update();
            m_effectManager.Update();
        }
        void OnDestroy()
        {
            m_timeManager.Dispose();
            m_effectManager.Dispose();
        }
        void Store(Infrastructure.Core.EventHandler onStoreOver)
        {
            EffectStoreData[] arr = new EffectStoreData[list.Count+listDelay.Count];
            for (int i = 0; i < list.Count; i++)
            {
                EffectStoreData data = new EffectStoreData();
                data.count = 5;
                data.index = i;
                data.group = EEffectGroup.Base;
                arr[i] = data;
            }
            for (int i = 0; i < listDelay.Count; i++)
            {
                EffectStoreData data = new EffectStoreData();
                data.count = 5;
                data.index = i+list.Count;
                data.group = EEffectGroup.Battle;
                arr[i + list.Count] = data;
            }
            EffectManager.Store(arr, onStoreOver);
        }
        void RandomAdd()
        {
            for (int m = 0; m < list.Count; m++)
            {
                usedTest.Add(EffectManager.Get(m,null,Vector3.zero,Quaternion.identity));
            }
        }
        void RandomAddDelay()
        {
            for (int m = 0; m < listDelay.Count; m++)
            {
                usedDelayTest.Add(EffectManager.Get(m+list.Count, null, Vector3.zero, Quaternion.identity, EEffectGroup.Battle));
            }
        }
        void RandomRemove()
        {
            for (int i = 0; i < usedTest.Count; i++)
            {
                EffectManager.Free(usedTest[i]);
            }
            for (int i = 0; i < usedDelayTest.Count; i++)
            {
                EffectManager.Free(usedDelayTest[i],1000,EEffectGroup.Battle);
            }
        }
        void OnLoad(int id,DataHandler<UnityEngine.Object> handler)
        {
            if (id<list.Count)
            {
                GameObject obj = Instantiate(list[id]);
                handler(obj);
            }
            else
            {
                GameObject obj = Instantiate(listDelay[id-list.Count]);
                handler(obj);
            }
        }
        void ONDestroy(int id)
        {
            if (id < list.Count)
            {
                Destroy(list[id]);
            }
            else
            {
                Destroy(listDelay[id - list.Count]);
            }
        }
    }
}