using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace lhFramework.UnitTest
{
    using System;
    using Infrastructure.Managers;
    using Infrastructure.Core;

    public class ModelPoolUnitTest : MonoBehaviour
    {
        public List<GameObject> list = new List<GameObject>();
        public List<GameObject> listDelay = new List<GameObject>();
        ModelManager m_modelManager;
        TimeManager m_timeManager;
        private List<IModel> usedTest = new List<IModel>();
        private List<IModel> usedDelayTest = new List<IModel>();
        // Use this for initialization
        void Start()
        {
            m_timeManager = TimeManager.GetInstance();
            m_modelManager = ModelManager.GetInstance();
            m_modelManager.loadHandler = OnLoad;
            m_modelManager.destroyHandler = ONDestroy;
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
            m_modelManager.Update();
        }
        void OnDestroy()
        {
            m_timeManager.Dispose();
            m_modelManager.Dispose();
        }
        void Store(Infrastructure.Core.EventHandler onStoreOver)
        {
            ModelStoreData[] arr = new ModelStoreData[list.Count+listDelay.Count];
            for (int i = 0; i < list.Count; i++)
            {
                ModelStoreData data = new ModelStoreData();
                data.count = 5;
                data.index = i;
                data.group = EModelGroup.Base;
                arr[i] = data;
            }
            for (int i = 0; i < listDelay.Count; i++)
            {
                ModelStoreData data = new ModelStoreData();
                data.count = 5;
                data.index = i+list.Count;
                data.group = EModelGroup.Battle;
                arr[i + list.Count] = data;
            }
            ModelManager.Store(arr, onStoreOver);
        }
        void RandomAdd()
        {
            for (int m = 0; m < list.Count; m++)
            {
                usedTest.Add(ModelManager.Get(m));
            }
        }
        void RandomAddDelay()
        {
            for (int m = 0; m < listDelay.Count; m++)
            {
                usedDelayTest.Add(ModelManager.Get(m+list.Count,EModelGroup.Battle));
            }
        }
        void RandomRemove()
        {
            for (int i = 0; i < usedTest.Count; i++)
            {
                ModelManager.Free(usedTest[i]);
            }
            for (int i = 0; i < usedDelayTest.Count; i++)
            {
                ModelManager.Free(usedDelayTest[i],1000, EModelGroup.Battle);
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